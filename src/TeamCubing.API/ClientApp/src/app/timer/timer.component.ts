// noinspection SpellCheckingInspection

import {Component, HostListener, Input, OnDestroy, OnInit, Output} from '@angular/core';
import {Observable, Subject, Subscription, timer} from 'rxjs';
import {MsToTimePipe} from '../pipes/ms-to-time.pipe';
import {PuzzleImage} from '../models/puzzles/puzzleImage';
import Utils from '../shared/utils';
import {Penalty} from '../models/solve';

export enum BackgroundColors {
  Red = 'bg-red',
  Green = 'bg-green',
}

export enum TimingMode {
  Timer,
  Manual
}

@Component({
  selector: 'app-timer',
  templateUrl: './timer.component.html',
  styleUrls: ['./timer.component.css'],
  providers: [MsToTimePipe],
})
export class TimerComponent implements OnInit, OnDestroy {
  @Output()
  public timerStopped: Subject<number> = new Subject<number>();

  @Output()
  public penaltySet: Subject<Penalty> = new Subject<Penalty>();

  @Input()
  public scramble: string = 'SCRAMBLE GENERATING...';

  @Input()
  public puzzleImage: PuzzleImage = Utils.threeByThreeSolvedImage;

  @Input()
  public reset: Observable<void> = new Observable<void>();

  public resetSub!: Subscription;

  public backgroundColorClass: string = '';

  public timeInMilliseconds: number = 0;

  public currentPenalty: Penalty = Penalty.NoPenalty;

  public timingMode = TimingMode;

  public currentTimingMode: TimingMode = TimingMode.Timer;

  public isRunning: boolean = false;
  public isTimerStopped: boolean = false;

  public readonly timerStep: number = 10;

  public ngOnInit(): void {
    this.resetSub = this.reset.subscribe({
      next: () => {
        this.fullReset()
      },
    });
    timer(0, this.timerStep).subscribe(() => {
      if (this.isRunning) {
        this.timeInMilliseconds += this.timerStep;
      }
    });
  }

  public get timeToString(): string {
    const toString = new MsToTimePipe();

    return toString.transform(this.timeInMilliseconds, this.isDnf());
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

  public dnfSolve(): void {
    this.currentPenalty = Penalty.DNF;
    this.penaltySet.next(this.currentPenalty);
  }

  public disablePenalty(): void {
    if (this.currentPenalty === Penalty.PlusTwo) {
      this.timeInMilliseconds -= 2000;
    }

    this.currentPenalty = Penalty.NoPenalty;
    this.penaltySet.next(this.currentPenalty);
  }

  public plusTwoSolve(): void {
    this.currentPenalty = Penalty.PlusTwo;
    this.timeInMilliseconds += 2000;
    this.penaltySet.next(this.currentPenalty);
  }

  public isNoPenalty(): boolean {
    return this.currentPenalty === Penalty.NoPenalty;
  }

  public isDnf(): boolean {
    return this.currentPenalty === Penalty.DNF;
  }

  public isPlusTwo(): boolean {
    return this.currentPenalty === Penalty.PlusTwo;
  }

  public isTimerMode(): boolean {
    return this.currentTimingMode === TimingMode.Timer;
  }

  public sendManualTime(): void {
    // Manual time entered as microseconds
    this.timerStopped.next(this.timeInMilliseconds * 10);

    this.resetTimerState();
    this.isTimerStopped = true;
  }

  private startTimer(): void {
    this.timeInMilliseconds = 0;
    this.isRunning = true;
    this.isTimerStopped = false;
    this.backgroundColorClass = BackgroundColors.Red;
  }

  private stopTimer(): void {
    this.timerStopped.next(this.timeInMilliseconds);

    this.resetTimerState();
    this.isTimerStopped = true;
  }

  private fullReset(): void {
    this.resetTimerState();
    this.timeInMilliseconds = 0;
  }

  private resetTimerState(): void {
    this.currentPenalty = Penalty.NoPenalty;
    this.isRunning = false;
    this.isTimerStopped = false;
    this.backgroundColorClass = '';
  }

  public ngOnDestroy() {
    this.resetSub.unsubscribe();
  }
}
