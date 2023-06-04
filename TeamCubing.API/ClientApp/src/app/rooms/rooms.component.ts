import { Component, EventEmitter, Input, OnDestroy, OnInit } from '@angular/core';
import { IRoomSolve, ISolveResult } from '../models/roomSolve';
import { MsToTimePipe } from '../pipes/ms-to-time.pipe';
import { RoomService } from '../services/room.service';
import { IRoomLoginResult } from '../models/roomLoginResult';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { Location } from '@angular/common';
import { TimerSolve } from '../models/TimerSolve';

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

  public currentRoomId: number = 0;
  public currentRoomName: string = '';
  public connectedUserNames: string[] = [];
  public solves: IRoomSolve[] = [];
  public currentSolveNumber: number = 0;
  public currentTime: number = 0;

  @Input()
  public currentScramble: string = 'SCRAMBLE GENERATING';

  public availableRooms: string[] = [];

  public subs$: Subscription[] = [];

  public timeChanged: EventEmitter<number> = new EventEmitter<number>();

  public isSolvePlusTwo: boolean = false;
  public isSolveDnf: boolean = false;

  public timeToNextSolve: number = 120;
  private interval!: any;

  public constructor(
    private roomService: RoomService,
    private route: ActivatedRoute,
    private router: Router,
    private location: Location,
  ) {}

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
      this.subs$.push(
        this.roomService.createRoom(this.roomNameInput, this.roomPasswordInput).subscribe({
          next: (response: boolean): void => {
            if (response) {
              this.joinRoom();
            } else {
              this.isRoomCreateFailed = true;
            }
          },
        }),
      );
    }
  }

  public onNewScramble($event: string): void {
    this.currentScramble = $event;
  }

  public joinRoom(): void {
    this.isLoginFailed = false;
    this.isRoomCreateFailed = false;

    this.subs$.push(
      this.roomService.loginToRoom(this.roomNameInput, this.roomPasswordInput).subscribe({
        next: (response: IRoomLoginResult): void => {
          if (response.result) {
            this.currentRoomName = this.roomNameInput;
            this.currentRoomId = response.roomId;
            this.connectedUserNames = response.connectedUserNames;

            this.solves = response.solves;
            const currentSolveNumber = response.solves.sort((one, two) =>
              one.solveNumber > two.solveNumber ? -1 : 1,
            )[0].solveNumber;
            if (currentSolveNumber) {
              this.currentSolveNumber = currentSolveNumber;
            }

            this.isAuthorized = true;

            this.setupRoomConnection();
            this.location.replaceState('/rooms/' + this.currentRoomName);
          } else {
            this.isLoginFailed = true;
          }
        },
        error: () => {
          this.isLoginFailed = true;
        },
      }),
    );
  }

  public sendResult(): void {
    const currentSolve = this.solves.find((s) => s.solveNumber === this.currentSolveNumber);

    if (currentSolve?.id) {
      this.roomService.sendResult(this.currentRoomId, currentSolve.id, this.currentTime);
      this.isSolveFinished = false;
    } else {
      console.debug('Current solve id empty');
    }
  }

  public getUserSolveTime(userName: string, solve: IRoomSolve): string {
    const solveTime = solve.results.find((s: ISolveResult) => s.userName === userName)?.time;
    if (solveTime) {
      const msToTimePipe = new MsToTimePipe();
      return msToTimePipe.transform(solveTime);
    }

    return '--:--';
  }

  public leaveRoom(): void {
    this.subs$.push(
      this.roomService.logoutRoom().subscribe({
        next: (response: boolean): void => {
          if (response) {
            this.roomService.leaveRoom(this.currentRoomName);
            this.router.navigate(['/rooms']);
          }
        },
      }),
    );
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

  public onTimerResult($event: TimerSolve): void {
    this.currentTime = $event.time;
    this.isSolveFinished = true;
  }

  public ngOnDestroy(): void {
    for (const sub of this.subs$) {
      sub.unsubscribe();
    }
    this.roomService.logoutRoom().subscribe({
      next: (response: boolean): void => {
        if (response) {
          this.roomService.leaveRoom(this.currentRoomName);
        }
      },
    });
  }

  private setupRoomConnection(): void {
    this.roomService.startConnection().then(() => {
      this.roomService.joinRoom(this.currentRoomName);
      this.roomService.subscribeOnAllRoomEvents();
      this.isLoaded = true;
      this.roomService.newSolve(this.currentRoomId);
    });

    this.subs$.push(
      this.roomService.users().subscribe({
        next: (userName: string): void => {
          if (!this.connectedUserNames.find((u: string) => u === userName)) {
            this.connectedUserNames.push(userName);
          }
        },
      }),
      this.roomService.leftUsers().subscribe({
        next: (userName: string): void => {
          this.connectedUserNames = this.connectedUserNames.filter((u) => u !== userName);
        },
      }),
      this.roomService.results().subscribe({
        next: (result: ISolveResult): void => {
          this.appendNewUserResult(result);
        },
      }),
      this.roomService.solveFinished().subscribe({
        next: (result: IRoomSolve) => {
          this.appendNewSolve(result);
          this.stopRoomSolveTimer();
          this.startRoomSolveTimer();
        },
      }),
    );
  }

  private appendNewSolve(solve: IRoomSolve): void {
    this.isSolveFinished = false;
    this.isSolveDnf = false;
    this.isSolvePlusTwo = false;
    this.currentTime = 0;
    this.timeChanged.emit(this.currentTime);

    this.currentSolveNumber = solve.solveNumber;
    this.solves.unshift(solve);
  }

  private appendNewUserResult(result: ISolveResult): void {
    const currentSolve: IRoomSolve | undefined = this.solves.find(
      (s: IRoomSolve): boolean => s.solveNumber === this.currentSolveNumber,
    );
    if (currentSolve) {
      currentSolve.results.push(result);
    }
  }

  private joinRoomIfHasAccess(roomName: string): void {
    this.roomNameInput = roomName;
    this.subs$.push(
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
      }),
    );
  }

  private loadAllRooms(): void {
    this.subs$.push(
      this.roomService.getAllRooms().subscribe({
        next: (response: string[]): void => {
          this.availableRooms = response;
          this.isLoaded = true;
        },
      }),
    );
  }

  private startRoomSolveTimer(): void {
    this.interval = setInterval(() => {
      if (this.timeToNextSolve > 0) {
        this.timeToNextSolve--;
      } else {
        this.roomService.forceNewSolve(this.currentRoomId);
        this.timeToNextSolve = 120;
      }
    }, 1000);
  }

  private stopRoomSolveTimer(): void {
    clearInterval(this.interval);
    this.timeToNextSolve = 120;
  }
}
