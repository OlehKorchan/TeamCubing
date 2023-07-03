import { Component, OnDestroy, OnInit } from '@angular/core';
import { Penalty, Solve, SolveResult } from '../../models/solve';
import { RoomService } from '../../services/room.service';
import { Room } from '../../models/room';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, Subscription } from 'rxjs';
import { Location } from '@angular/common';
import { ConfigurationService } from '../../shared/services/configuration.service';
import { NgxSpinnerService } from 'ngx-spinner';
import { ModelResponse } from '../../models/modelResponse';
import { AuthenticationService } from '../../modules/authentication/services/authentication.service';
import { MsToTimePipe } from '../../pipes/ms-to-time.pipe';
import { SolveService } from '../../services/solve.service';
import { not } from 'rxjs/internal/util/not';
import { MatDialog } from '@angular/material/dialog';
import { CreateRoomDialogComponent } from './create-room-dialog/create-room-dialog.component';
import { RoomLoginRequest } from '../../models/roomLoginRequest';
import { RoomDisplayDataResponse } from '../../models/roomDisplayDataResponse';
import { RoomPuzzle } from '../../models/roomSettings';
import { JoinRoomDialogComponent } from './join-room-dialog/join-room-dialog.component';
import Utils from '../../shared/utils';
import { SolveInfoComponent } from '../solve-info/solve-info.component';

@Component({
  selector: 'app-rooms',
  templateUrl: './rooms.component.html',
  styleUrls: ['./rooms.component.css'],
})
export class RoomsComponent implements OnInit, OnDestroy {
  private readonly EmptyScrambleMessage: string = 'Waiting for scramble...';

  public isLoaded: boolean = false;
  public isAuthorized: boolean = false;

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
    scramble: this.EmptyScrambleMessage,
    scrambledPuzzleImage: Utils.threeByThreeSolvedImage,
    startTime: new Date(),
  };

  public availableRooms: RoomDisplayDataResponse[] = [];
  public reset: Subject<void> = new Subject<void>();

  public subs$: Subscription[] = [];

  public timeToNextSolve: number = 0;
  public mean: { [user: string]: { time: string } } = {};
  protected readonly not = not;
  private interval!: any;
  private readonly emptyTime: string = '--:--';
  public averages: { [user: string]: { ao: number; time: string }[] } = {};
  public availableAverages: number[] = [5, 12, 50, 100];

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

  // public get notEmptyAverages(): { ao: number; isOn: boolean; time: string }[] {
  //   return this.averages.flatMap((a) => (a.isOn ? a : []));
  // }

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

  public onSendResult($event: SolveResult): void {
    if (this.currentSolve) {
      this.roomService.sendResult({
        roomId: this.room.id,
        solveNumber: this.currentSolve.solveNumber,
        timeInMilliseconds: $event.time,
        penalty: $event.penalty,
      });

      this.currentSolve.scramble = this.EmptyScrambleMessage;
      this.reset.next();
    } else {
      console.error('Current solve empty');
    }
  }

  public getUserResult(solve: Solve, user: string): SolveResult | undefined {
    return solve.results?.find((s: SolveResult) => s.userName === user);
  }

  public openSolveInfo(solve: Solve, time: string): void {
    this.dialog.open(SolveInfoComponent, {
      data: {
        solve: solve,
        time: time,
      },
      width: '300px',
    });
  }

  public colorUserResult(user: string, solve: Solve): 'red' | 'green' | 'black' {
    const allUserResults = this.solveService.getNonDnfUserResultsSorted(
      this.room.solves,
      user,
      'ascending',
    );
    const bestResult = allUserResults.at(0);
    const worstResult = allUserResults.at(-1);
    const currentResult = this.getUserResult(solve, user);

    if (currentResult) {
      if (bestResult?.time === currentResult.time) {
        return 'green';
      }

      if (worstResult?.time === currentResult.time) {
        return 'red';
      }
    }

    return 'black';
  }

  public formatResult(result: SolveResult | undefined): string {
    if (result?.time) {
      const msToTimePipe = new MsToTimePipe();
      const isDnf = result.penalty === Penalty.DNF;

      return msToTimePipe.transform(result.time, isDnf);
    }

    return this.emptyTime;
  }

  public loadAllResults(): void {
    if (this.room?.solves?.length) {
      const currentSolve = this.room.solves.at(0);
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
        this.room.solves = room.solves.sort((one, two) =>
          one.solveNumber > two.solveNumber ? -1 : 1,
        );
        this.loadAllResults();

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
    this.reset.next();

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
    this.averages = {};

    for (const user of this.room.connectedUserNames) {
      const mean = this.solveService.calculateMean(0, this.room.solves, user);
      if (mean as number) {
        this.mean[user] = { time: toString.transform(mean) };
      } else {
        this.mean[user] = { time: 'n/a' };
      }

      for (const n of this.availableAverages) {
        const aoN = this.solveService.calculateAverage(n, this.room.solves, user);
        const newAvg: { ao: number; time: string } = { ao: n, time: 'n/a' };

        if (aoN === this.config.dnfValue) {
          newAvg.time = toString.transform(aoN, true, true);
        } else if (aoN > 0) {
          newAvg.time = toString.transform(aoN);
        }

        if (!this.averages[user]) {
          this.averages[user] = [];
        }

        this.averages[user].push(newAvg);
      }
    }
  }
}
