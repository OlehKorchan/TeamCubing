import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import cubeScrambler from 'cube-scrambler';
import { Observable } from 'rxjs';
import { TimerSolve } from '../models/TimerSolve';

@Component({
  selector: 'app-scramble',
  templateUrl: './scramble.component.html',
  styleUrls: ['./scramble.component.css'],
})
export class ScrambleComponent implements OnInit {
  @Input()
  public newScramble!: Observable<TimerSolve>;

  @Output()
  public generatedScramble: EventEmitter<string> = new EventEmitter<string>();

  public scramble!: string;

  public ngOnInit(): void {
    this.generateScramble();
    this.newScramble.subscribe({
      next: () => this.generateScramble(),
    });
  }

  public generateScramble(): void {
    this.scramble =
      (
        cubeScrambler().scramble() as string[]
      ).toString().replace(/,/g, ' ');
    this.generatedScramble.emit(this.scramble);
  }
}
