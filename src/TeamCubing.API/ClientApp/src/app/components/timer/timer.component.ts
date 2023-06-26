import {
  Component,
  ElementRef,
  Host,
  HostListener,
  Input,
  OnDestroy,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import { Observable, Subject, Subscription, timer } from 'rxjs';
import { MsToTimePipe } from '../../pipes/ms-to-time.pipe';
import { PuzzleImage } from '../../models/puzzles/puzzleImage';
import Utils from '../../shared/utils';
import { Penalty, SolveResult } from '../../models/solve';
import { RoomPuzzle } from '../../models/roomSettings';

export enum BackgroundColors {
  Red = 'bg-red',
  Green = 'bg-green',
}

export enum TimingMode {
  Timer,
  Manual,
}

@Component({
  selector: 'app-timer',
  templateUrl: './timer.component.html',
  styleUrls: ['./timer.component.css'],
  providers: [MsToTimePipe],
})
export class TimerComponent implements OnInit, OnDestroy {
  @Output()
  public sendResult: Subject<SolveResult> = new Subject<SolveResult>();

  @Output()
  public timerStopped: Subject<number> = new Subject<number>();

  @Output()
  public penaltySet: Subject<Penalty> = new Subject<Penalty>();

  @Input()
  public scramble: string = 'SCRAMBLE GENERATING...';

  @Input()
  public puzzleImage: PuzzleImage = Utils.threeByThreeSolvedImage;

  @Input()
  public puzzleType: RoomPuzzle = RoomPuzzle.ThreeByThreeCube;

  @Input()
  public reset: Observable<void> = new Observable<void>();

  public resetSub!: Subscription;

  public backgroundColorClass: string = '';

  public timeInMilliseconds: number = 0;
  public inputTime: number | undefined = undefined;

  public currentPenalty: Penalty = Penalty.NoPenalty;

  public timingMode = TimingMode;

  public currentTimingMode: TimingMode = TimingMode.Timer;

  public isRunning: boolean = false;
  public isTimerStopped: boolean = false;

  public readonly timerStep: number = 10;

  @ViewChild('manualInput')
  public manualInput!: ElementRef;

  public ngOnInit(): void {
    this.resetSub = this.reset.subscribe({
      next: () => {
        this.fullReset();
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

  @HostListener('document:keyup.enter', ['$event'])
  public onSendResult(event: Event): void {
    this.sendTime();
    event.preventDefault();
  }

  @HostListener('document:keydown.space', ['$event'])
  public onTimerReady(event: Event): void {
    if (this.currentTimingMode === TimingMode.Timer) {
      if (!this.isRunning) {
        this.backgroundColorClass = BackgroundColors.Green;
      }
      event.preventDefault();
    }
  }

  @HostListener('document:keyup.space', ['$event'])
  public onTimerFinished(event: Event): void {
    if (this.currentTimingMode === TimingMode.Timer) {
      this.toggleTimer();
      event.preventDefault();
    }
  }

  public toggleTimer(): void {
    if (this.isRunning) {
      this.stopTimer();
    } else {
      this.startTimer();
    }
  }

  public focusManualInput(): void {
    if (this.currentTimingMode === TimingMode.Manual && this.manualInput) {
      this.manualInput.nativeElement.focus();
    }
  }

  public dnfSolve(): void {
    this.currentPenalty = Penalty.DNF;
    this.penaltySet.next(this.currentPenalty);
  }

  public disablePenalty(): void {
    if (this.currentPenalty === Penalty.PlusTwo) {
      this.timeInMilliseconds -= 2000;
      if (this.currentTimingMode === TimingMode.Manual && this.inputTime) {
        this.inputTime -= 200;
      }
    }

    this.currentPenalty = Penalty.NoPenalty;
    this.penaltySet.next(this.currentPenalty);
  }

  public plusTwoSolve(): void {
    this.currentPenalty = Penalty.PlusTwo;
    this.timeInMilliseconds += 2000;
    if (this.currentTimingMode === TimingMode.Manual && this.inputTime) {
      this.inputTime += 200;
    }
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

  public get isManualMode(): boolean {
    return this.currentTimingMode === TimingMode.Manual;
  }

  public updateTime(): void {
    if (this.inputTime && this.inputTime > 0) {
      const microseconds = this.inputTime % 100;
      const seconds = parseInt(((this.inputTime % 10000) / 100).toString());
      const minutes = parseInt(((this.inputTime % 1000000) / 10000).toString());
      const hours = parseInt(((this.inputTime % 100000000) / 1000000).toString());
      this.timeInMilliseconds =
        microseconds * 10 + seconds * 1000 + minutes * 60000 + hours * 3600000;
      this.timerStopped.next(this.timeInMilliseconds);

      this.isTimerStopped = true;
    } else {
      this.isTimerStopped = false;
      this.timeInMilliseconds = 0;
    }
  }

  public sendTime(): void {
    if (this.timeInMilliseconds > 0 || this.currentPenalty === Penalty.DNF) {
      this.sendResult.next({
        time: this.timeInMilliseconds,
        penalty: this.currentPenalty,
        userName: '',
      });
      this.fullReset();
    }
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
    this.inputTime = undefined;
  }

  private resetTimerState(): void {
    this.currentPenalty = Penalty.NoPenalty;
    this.isRunning = false;
    this.isTimerStopped = false;
    this.backgroundColorClass = '';
    if (this.currentTimingMode === TimingMode.Manual) {
      this.manualInput.nativeElement.focus();
    }
  }

  public ngOnDestroy() {
    this.resetSub.unsubscribe();
  }

  protected readonly undefined = undefined;
}
