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

@Component({
  selector: 'app-rooms',
  templateUrl: './rooms.component.html',
  styleUrls: ['./rooms.component.css'],
})
export class RoomsComponent implements OnInit, OnDestroy {
  public isLoaded: boolean = false;
  public isAuthorized: boolean = false;
  public isSolveFinished: boolean = false;

  public roomNameInput: string = '';
  public roomPasswordInput: string = '';

  public isLoginFailed: boolean = false;
  public isRoomCreateFailed: boolean = false;

  public room: Room = {
    solves: [],
    connectedUserNames: [],
    name: '',
    id: '',
    wasOnceConnectedUserNames: [],
  };
  public currentSolve: Solve = {
    results: [],
    solveNumber: 0,
    scramble: 'SCRAMBLE GENERATING',
    startTime: new Date(),
  };
  public currentTime: number = 0;

  public availableRooms: string[] = [];

  public subs$: Subscription[] = [];

  public timeChanged: EventEmitter<number> = new EventEmitter<number>();

  public isSolvePlusTwo: boolean = false;
  public isSolveDnf: boolean = false;

  public timeToNextSolve: number = 0;
  private interval!: any;
  private readonly emptyTime: string = '--:--';

  public constructor(
    private spinner: NgxSpinnerService,
    private roomService: RoomService,
    private route: ActivatedRoute,
    private router: Router,
    private location: Location,
    private config: ConfigurationService,
    private auth: AuthenticationService,
    private solveService: SolveService,
  ) {
    this.timeToNextSolve = config.getTimeToNextSolve();
  }

  public get currentUserName(): string {
    return this.auth.getUserName();
  }

  public get remainingPercents(): number {
    return (this.timeToNextSolve / this.config.getTimeToNextSolve()) * 100;
  }

  public bestUserResult(user: string = ''): SolveResult {
    if (user === '') {
      user = this.currentUserName;
    }

    const result = this.room.solves
      ?.flatMap((s) => {
        return s.results?.find((r) => r.userName === user);
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

  public ngOnInit(): void {
    const roomName = this.route.snapshot.paramMap.get('roomName');
    if (roomName) {
      this.joinRoomIfHasAccess(roomName);
    } else {
      this.loadAllRooms();
    }
  }

  public createNewRoom(): void {
    this.isRoomCreateFailed = false;
    this.isLoginFailed = false;
    if (this.roomNameInput && this.roomPasswordInput) {
      this.roomService.createRoom(this.roomNameInput, this.roomPasswordInput).subscribe({
        next: (response: boolean): void => {
          if (response) {
            this.joinRoom();
          } else {
            this.isRoomCreateFailed = true;
          }
        },
      });
    }
  }

  public joinRoom(): void {
    this.isLoginFailed = false;
    this.isRoomCreateFailed = false;

    this.roomService.loginToRoom(this.roomNameInput, this.roomPasswordInput).subscribe({
      next: (response: ModelResponse<Room>): void => {
        if (response.isSuccess) {
          this.setupRoomConnection(response.model.name, response.model.id).then(() => {
            if (this.isLoaded) {
              this.room = response.model;
              this.tryGetLastSolveFromRoomSolves();

              this.location.replaceState('/rooms/' + this.room.name);
              this.isAuthorized = true;
            } else {
              this.isLoginFailed = true;
              this.router.navigate(['/rooms']);
            }
          });
        } else {
          this.isLoginFailed = true;
        }
      },
      error: () => {
        this.isLoginFailed = true;
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
    if (this.room.solves?.length) {
      const currentSolve = this.room.solves.sort((one, two) =>
        one.solveNumber > two.solveNumber ? -1 : 1,
      )[0];
      if (currentSolve) {
        this.currentSolve = currentSolve;
      }
      this.roomService.askForNewSolve(this.room.id);
    } else {
      this.roomService.forceNewSolve(this.room.id);
    }
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
    const currentUserResults = this.solveService.getCurrentUserResultsFromRoom(this.room);

    const mean = this.solveService.calculateMean(0, currentUserResults);

    if (mean <= 0) {
      return 'n/a';
    }

    const toTime = new MsToTimePipe();

    return toTime.transform(mean);
  }

  public getAverage(n: number): string {
    const currentUserResults = this.solveService.getCurrentUserResultsFromRoom(this.room);

    const average = this.solveService.calculateAverage(n, currentUserResults);

    if (average <= 0) {
      return 'n/a';
    }

    const toTime = new MsToTimePipe();

    return toTime.transform(average);
  }

  public ngOnDestroy(): void {
    for (const sub of this.subs$) {
      sub.unsubscribe();
    }
    this.roomService.leaveRoom(this.room.name);
  }

  private async setupRoomConnection(roomName: string, roomId: string): Promise<any> {
    await this.roomService.startConnection();

    this.isLoaded = true;
    this.roomService.joinRoom(roomName);

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
          this.room.connectedUserNames = this.room.connectedUserNames.filter((u) => u !== userName);
        },
      }),
      this.roomService.results().subscribe({
        next: (result: SolveResult): void => {
          this.appendNewUserResult(result);
        },
      }),
      this.roomService.solveFinished().subscribe({
        next: (result: Solve) => {
          this.appendNewSolve(result);
          this.resetSolveTimer();
          this.startSolveTimer();
        },
      }),
    );
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

  private joinRoomIfHasAccess(roomName: string): void {
    this.roomNameInput = roomName;
    this.roomService.checkAccessToRoom(roomName).subscribe({
      next: (response: boolean) => {
        if (response) {
          this.joinRoom();
        } else {
          this.loadAllRooms();
          this.isLoaded = true;
          this.roomNameInput = roomName;
        }
      },
    });
  }

  private loadAllRooms(): void {
    this.roomService.getAllRooms().subscribe({
      next: (response: string[]): void => {
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
        this.roomService.forceNewSolve(this.room.id);
        this.timeToNextSolve = this.config.getTimeToNextSolve();
      }
    }, 1000);
  }

  private resetSolveTimer(): void {
    clearInterval(this.interval);
    this.timeToNextSolve = this.config.getTimeToNextSolve();
  }
}
