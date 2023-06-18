import { Component, HostListener, Input, OnInit, Output } from '@angular/core';
import { Observable, Subject, timer } from 'rxjs';
import { MsToTimePipe } from '../pipes/ms-to-time.pipe';

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
  public timerStopped: Subject<number> = new Subject<number>();

  @Input()
  public timeChanged!: Observable<number>;

  @Input()
  public scramble: string = 'SCRAMBLE GENERATING...';

  public backgroundColorClass: string = '';

  public timeInMilliseconds: number = 0;

  public isRunning: boolean = false;

  public readonly timerStep: number = 10;

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

  private startTimer(): void {
    this.timeInMilliseconds = 0;
    this.isRunning = true;
    this.backgroundColorClass = BackgroundColors.Red;
  }

  private stopTimer(): void {
    this.isRunning = false;
    this.backgroundColorClass = '';
    this.timerStopped.next(this.timeInMilliseconds);
  }
}
