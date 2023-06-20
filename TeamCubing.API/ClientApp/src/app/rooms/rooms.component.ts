import { Component, EventEmitter, OnDestroy, OnInit } from '@angular/core';
import { Solve, SolveResult } from '../models/solve';
import { RoomService } from '../services/room.service';
import { Room } from '../models/room';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { Location } from '@angular/common';
import { ConfigurationService } from '../shared/services/configuration.service';
import { NgxSpinnerService } from 'ngx-spinner';
import { ModelResponse } from '../models/modelResponse';
import { AuthenticationService } from '../modules/authentication/services/authentication.service';
import { MsToTimePipe } from '../pipes/ms-to-time.pipe';
import { SolveService } from '../services/solve.service';
import { not } from 'rxjs/internal/util/not';
import { MatDialog } from '@angular/material/dialog';
import { CreateRoomDialogComponent } from './create-room-dialog/create-room-dialog.component';
import { RoomLoginRequest } from '../models/roomLoginRequest';
import { RoomDisplayDataResponse } from '../models/roomDisplayDataResponse';
import { RoomPuzzle } from '../models/roomSettings';
import { JoinRoomDialogComponent } from './join-room-dialog/join-room-dialog.component';

@Component({
  selector: 'app-rooms',
  templateUrl: './rooms.component.html',
  styleUrls: ['./rooms.component.css'],
})
export class RoomsComponent implements OnInit, OnDestroy {
  public isLoaded: boolean = false;
  public isAuthorized: boolean = false;
  public isSolveFinished: boolean = false;

  public room: Room = {
    solves: [],
    connectedUserNames: [],
    name: '',
    id: '',
    wasOnceConnectedUserNames: [],
    settings: {
      puzzle: RoomPuzzle.ThreeByThreeCube,
      enableSolveTimeLimit: true,
      isOpen: false,
      usersLimit: 3,
    },
  };
  public currentSolve: Solve = {
    results: [],
    solveNumber: 0,
    scramble: 'SCRAMBLE GENERATING',
    startTime: new Date(),
  };
  public currentTime: number = 0;

  public availableRooms: RoomDisplayDataResponse[] = [];

  public subs$: Subscription[] = [];

  public timeChanged: EventEmitter<number> = new EventEmitter<number>();

  public isSolvePlusTwo: boolean = false;
  public isSolveDnf: boolean = false;

  public timeToNextSolve: number = 0;
  public mean: string = 'n/a';
  protected readonly not = not;
  private interval!: any;
  private readonly emptyTime: string = '--:--';
  private averages: { ao: number; isOn: boolean; time: string }[] = [
    { ao: 5, isOn: false, time: 'n/a' },
    { ao: 12, isOn: false, time: 'n/a' },
    { ao: 50, isOn: false, time: 'n/a' },
    { ao: 100, isOn: false, time: 'n/a' },
  ];

  public constructor(
    private spinner: NgxSpinnerService,
    private roomService: RoomService,
    private route: ActivatedRoute,
    private router: Router,
    private location: Location,
    private config: ConfigurationService,
    private auth: AuthenticationService,
    public solveService: SolveService,
    public dialog: MatDialog,
  ) {
    this.timeToNextSolve = config.getTimeToNextSolve();
  }

  public get notEmptyAverages(): { ao: number; isOn: boolean; time: string }[] {
    return this.averages.flatMap((a) => (a.isOn ? a : []));
  }

  public get currentUserName(): string {
    return this.auth.getUserName();
  }

  public get remainingPercents(): number {
    return (this.timeToNextSolve / this.config.getTimeToNextSolve()) * 100;
  }

  public ngOnInit(): void {
    this.route.paramMap.subscribe({
      next: (param) => {
        this.isAuthorized = false;
        const roomName = param.get('roomName');
        if (roomName) {
          this.joinRoomIfHasAccess(roomName);
        } else {
          this.loadAllRooms();
        }
      },
    });
  }

  public bestUserResult(user: string = ''): SolveResult {
    if (user === '') {
      user = this.currentUserName;
    }

    const result = this.room.solves
      ?.flatMap((s) => {
        return s.results?.find((r) => r.userName === user && r.time > 0);
      })
      ?.sort((one, two) => {
        if (one?.time && two?.time) {
          return one?.time > two?.time ? 1 : -1;
        }

        return 1;
      })[0];

    if (result) {
      return result;
    }

    return {
      time: 0,
      userName: user,
    };
  }

  public createNewRoom(): void {
    const dialogItem = this.dialog.open(CreateRoomDialogComponent, {
      width: '300px',
    });

    dialogItem.afterClosed().subscribe((data: RoomLoginRequest) => {
      if (data) {
        this.router.navigate(['/rooms', data.roomName]);
      }
    });
  }

  public openRoomJoinDialog(roomName: string): void {
    this.dialog
      .open(JoinRoomDialogComponent, {
        data: {
          roomName: roomName,
        },
        width: '300px',
      })
      .afterClosed()
      .subscribe({
        next: (response) => {
          const room = response as Room;
          if (room) {
            this.setupRoomConnection(room);
          } else {
            this.router.navigate(['/rooms']);
          }
        },
      });
  }

  public loginAndConnectToRoom(roomName: string, roomPassword?: string): void {
    const request: RoomLoginRequest = {
      roomName: roomName,
      roomPassword: roomPassword,
    };

    this.roomService.loginToRoom(request).subscribe({
      next: (response: ModelResponse<Room>): void => {
        if (response.isSuccess) {
          this.setupRoomConnection(response.model);
        }
      },
    });
  }

  public sendResult(): void {
    if (this.currentSolve) {
      this.roomService.sendResult(this.room.id, this.currentSolve.solveNumber, this.currentTime);
      this.isSolveFinished = false;
    } else {
      console.error('Current solve empty');
    }
  }

  public isBestUserSolve(userName: string, solve: Solve): boolean {
    const bestResult = this.bestUserResult(userName);

    const currentResult = solve.results?.find((s) => s.userName === userName)?.time;

    if (bestResult && currentResult) {
      return bestResult.time === currentResult;
    }

    return false;
  }

  public getUserSolveTime(userName: string, solve: Solve): string {
    const solveTime = solve.results?.find((s: SolveResult) => s.userName === userName)?.time;
    if (solveTime) {
      const msToTimePipe = new MsToTimePipe();
      return msToTimePipe.transform(solveTime);
    }

    return this.emptyTime;
  }

  public leaveRoom(): void {
    this.roomService.logoutRoom().subscribe({
      next: (response: boolean): void => {
        if (response) {
          this.roomService.leaveRoom(this.room.name);
          this.router.navigate(['/rooms']);
        }
      },
    });
  }

  public dnfSolve(): void {
    if (!this.isSolvePlusTwo && !this.isSolveDnf) {
      this.currentTime = -this.currentTime;
      this.timeChanged.next(this.currentTime);
      this.isSolveDnf = true;
    }
  }

  public disableDnfSolve(): void {
    if (this.isSolveDnf) {
      this.isSolveDnf = false;
      this.currentTime = -this.currentTime;
      this.timeChanged.next(this.currentTime);
    }
  }

  public plusTwoSolve(): void {
    if (!this.isSolvePlusTwo && !this.isSolveDnf) {
      this.currentTime += 2000;
      this.timeChanged.next(this.currentTime);
      this.isSolvePlusTwo = true;
    }
  }

  public disablePlusTwoSolve(): void {
    if (this.isSolvePlusTwo) {
      this.currentTime -= 2000;
      this.timeChanged.next(this.currentTime);
      this.isSolvePlusTwo = false;
    }
  }

  public onTimerResult($event: number): void {
    this.currentTime = $event;
    this.isSolveFinished = true;
  }

  public tryGetLastSolveFromRoomSolves(): void {
    if (this.room?.solves?.length) {
      const currentSolve = this.room.solves.sort((one, two) =>
        one.solveNumber > two.solveNumber ? -1 : 1,
      )[0];
      if (currentSolve) {
        this.currentSolve = currentSolve;
        this.recalculateAverages();
      }
    }

    this.roomService.askForNewSolve(this.room.id);
  }

  public getSolveProgressColor(): 'primary' | 'accent' | 'warn' {
    if (this.timeToNextSolve >= this.config.getTimeToNextSolve() / 2) {
      return 'primary';
    } else if (this.timeToNextSolve >= this.config.getTimeToNextSolve() / 4) {
      return 'accent';
    } else {
      return 'warn';
    }
  }

  public getMean(): string {
    const mean = this.solveService.calculateMean(0, this.room.solves);

    if (mean <= 0) {
      return 'n/a';
    }

    const toTime = new MsToTimePipe();

    return toTime.transform(mean);
  }

  public getAverage(n: number): string {
    const average = this.solveService.calculateAverage(n, this.room.solves);

    if (average <= 0) {
      return 'n/a';
    }

    const toTime = new MsToTimePipe();

    return toTime.transform(average);
  }

  public forceNewSolve(): void {
    this.roomService.forceNewSolve(this.room.id);
  }

  public joinRoomIfHasAccess(roomName: string): void {
    this.roomService.checkAccessToRoom(roomName).subscribe({
      next: (response: boolean) => {
        if (response) {
          this.loginAndConnectToRoom(roomName);
        } else {
          this.openRoomJoinDialog(roomName);
        }
      },
    });
  }

  public ngOnDestroy(): void {
    for (const sub of this.subs$) {
      sub.unsubscribe();
    }
    this.roomService.leaveRoom(this.room.name);
  }

  private setupRoomConnection(room: Room): void {
    this.roomService
      .startConnection()
      .then(() => {
        this.roomService.joinRoom(room.name);
        this.isLoaded = true;

        this.room = room;
        this.tryGetLastSolveFromRoomSolves();

        this.isAuthorized = true;

        this.roomService.subscribeOnAllRoomEvents();

        this.subs$.push(
          this.roomService.users().subscribe({
            next: (userName: string): void => {
              if (!this.room.connectedUserNames.find((u: string) => u === userName)) {
                this.room.connectedUserNames.push(userName);
              }
            },
          }),
          this.roomService.leftUsers().subscribe({
            next: (userName: string): void => {
              this.room.connectedUserNames = this.room.connectedUserNames.filter(
                (u) => u !== userName,
              );
            },
          }),
          this.roomService.results().subscribe({
            next: (result: SolveResult): void => {
              this.appendNewUserResult(result);

              if (result.userName === this.currentUserName) {
                this.recalculateAverages();
              }
            },
          }),
          this.roomService.solveFinished().subscribe({
            next: (result: Solve) => {
              this.appendNewSolve(result);

              if (this.room?.settings?.enableSolveTimeLimit) {
                this.resetSolveTimer();
                this.startSolveTimer();
              }
            },
          }),
        );
      })
      .catch(() => {
        this.router.navigate(['/rooms']);
      });
  }

  private appendNewSolve(solve: Solve): void {
    this.isSolveFinished = false;
    this.isSolveDnf = false;
    this.isSolvePlusTwo = false;
    this.currentTime = 0;
    this.timeChanged.emit(this.currentTime);

    this.currentSolve = solve;
    this.room.solves.unshift(solve);
  }

  private appendNewUserResult(result: SolveResult): void {
    if (this.currentSolve) {
      if (this.currentSolve.results?.length) {
        this.currentSolve.results.push(result);
      } else {
        this.currentSolve.results = [result];
      }
    }
  }

  private loadAllRooms(): void {
    this.roomService.getAllRooms().subscribe({
      next: (response: RoomDisplayDataResponse[]): void => {
        this.availableRooms = response;
        this.isLoaded = true;
      },
    });
  }

  private startSolveTimer(): void {
    this.interval = setInterval(() => {
      if (this.timeToNextSolve > 0) {
        this.timeToNextSolve--;
      } else {
        this.roomService.askForNewSolve(this.room.id);
        this.timeToNextSolve = this.config.getTimeToNextSolve();
      }
    }, 1000);
  }

  private resetSolveTimer(): void {
    clearInterval(this.interval);
    this.timeToNextSolve = this.config.getTimeToNextSolve();
  }

  private recalculateAverages(): void {
    const toString = new MsToTimePipe();
    const mean = this.solveService.calculateMean(0, this.room.solves);

    if (mean > 0) {
      this.mean = toString.transform(mean);
    }

    for (const n of [5, 12, 50, 100]) {
      const aoN = this.solveService.calculateAverage(n, this.room.solves);
      let avg = this.averages.find((a) => a.ao === n);
      if (!avg) {
        avg = {
          ao: n,
          isOn: false,
          time: 'n/a',
        };
        this.averages.push(avg);
      }

      if (aoN === this.config.dnfValue || aoN > 0) {
        avg.isOn = true;
        avg.time = toString.transform(aoN);
      } else {
        avg.isOn = false;
        avg.time = 'n/a';

        return;
      }
    }
  }
}
