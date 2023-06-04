import { Component, EventEmitter, HostListener, Input, OnInit, Output } from '@angular/core';
import { Observable, Subject, timer } from 'rxjs';
import { MsToTimePipe } from '../pipes/ms-to-time.pipe';
import { SolveService } from '../services/solve.service';
import { DialogService } from '../services/dialog.service';
import { TimerSolve } from '../models/TimerSolve';

export enum BackgroundColors {
  Red = 'bg-red',
  Green = 'bg-green',
}

@Component({
  selector: 'app-timer',
  templateUrl: './timer.component.html',
  styleUrls: ['./timer.component.css'],
  providers: [MsToTimePipe],
})
export class TimerComponent implements OnInit {
  @Output()
  public scrambleGenerated: EventEmitter<string> = new EventEmitter<string>();

  @Output()
  public timerStopped: Subject<TimerSolve> = new Subject<TimerSolve>();

  @Input()
  public timeChanged!: Observable<number>;

  public currentScramble!: string;

  public backgroundColorClass: string = '';

  public timeInMilliseconds: number = 0;

  public isRunning: boolean = false;

  // public currentSession!: Session;

  public mean: number = 0;

  public readonly averagesKeys: number[] = [5, 12, 50, 100, 1000];

  public averages: { [key: number]: number } = {};

  public readonly timerStep: number = 10;

  public constructor(
    private msToTimePipe: MsToTimePipe,
    private solveService: SolveService,
    private dialogService: DialogService,
  ) {}

  // public get solves(): Solve[] {
  //   let solves = this.currentSession?.solves ?? [];
  //
  //   let index = solves.length;
  //
  //   solves = solves.map<Solve>((s) => {
  //     s.position = index;
  //     index -= 1;
  //
  //     return s;
  //   });
  //
  //   return solves;
  // }

  public ngOnInit(): void {
    if (this.timeChanged) {
      this.timeChanged.subscribe({
        next: (value: number) => {
          this.timeInMilliseconds = value;
        },
      });
    }
    timer(0, this.timerStep).subscribe(() => {
      if (this.isRunning) {
        this.timeInMilliseconds += this.timerStep;
      }
    });
  }

  // public onChangeCurrentSession(newSession: Session) {
  //   this.currentSession = newSession;
  //
  //   this.calculateAverage();
  // }

  public onScrambleGenerated($event: string): void {
    this.currentScramble = $event;
    this.scrambleGenerated.emit(this.currentScramble);
  }

  public onTimeChanged($event: number): void {
    this.timeInMilliseconds = $event;
  }

  @HostListener('document:keydown.space', ['$event'])
  public onTimerReady(event: Event): void {
    if (!this.isRunning) {
      this.backgroundColorClass = BackgroundColors.Green;
    }
    event.preventDefault();
  }

  @HostListener('document:keyup.space', ['$event'])
  public onTimerFinished(event: Event): void {
    this.toggleTimer();
    event.preventDefault();
  }

  public toggleTimer(): void {
    if (this.isRunning) {
      this.stopTimer();
    } else {
      this.startTimer();
    }
  }

  // public onSolveRemove(id: number) {
  //   this.currentSession.solves = this.currentSession.solves.filter((s: Solve) => s.id !== id);
  //   this.calculateAverage();
  //   this.timerStopped.next();
  // }

  // public onClearSession(): void {
  //   this.currentSession.solves = [];
  //   this.clearAverages();
  //   this.timerStopped.next();
  // }

  private startTimer(): void {
    this.timeInMilliseconds = 0;
    this.isRunning = true;
    this.backgroundColorClass = BackgroundColors.Red;
  }

  private stopTimer(): void {
    this.isRunning = false;
    this.backgroundColorClass = '';
    this.timerStopped.next({
      time: this.timeInMilliseconds,
    });

    // const newSolve: Solve = {
    //   id: 0,
    //   time: this.timeInMilliseconds,
    //   scramble: this.currentScramble,
    //   sessionId: this.currentSession.id,
    // };
    // this.solveService.saveUserSolve(newSolve).subscribe({
    //   next: (solve: Solve) => {
    //     this.currentSession.solves.unshift(solve);
    //     this.timerStopped.next({
    //       time: this.timeInMilliseconds,
    //     });
    //     this.calculateAverage();
    //   },
    // });
  }

  private clearAverages(): void {
    this.mean = 0;
    this.averages = {};
  }

  // private calculateAverage(): void {
  //   // const listLength = this.currentSession.solves.length;
  //
  //   if (listLength) {
  //     this.mean = Math.round(
  //       this.currentSession.solves.map<number>((s) => s.time).reduce((a, b) => a + b, 0) /
  //         listLength,
  //     );
  //   }
  //
  //   this.averages = {};
  //   this.averagesKeys.forEach((key) => {
  //     if (listLength >= key) {
  //       this.averages[key] = this.calculateAoN(key);
  //     }
  //   });
  // }

  // private calculateAoN(n: number): number {
  //   let firstN = this.currentSession.solves.slice(0, n).map<number>((s) => s.time);
  //   const min = Math.min(...firstN);
  //   const max = Math.max(...firstN);
  //
  //   firstN = firstN.filter((el) => el !== min && el !== max);
  //
  //   return Math.round(firstN.reduce((a, b) => a + b, 0) / (n - 2));
  // }
}
