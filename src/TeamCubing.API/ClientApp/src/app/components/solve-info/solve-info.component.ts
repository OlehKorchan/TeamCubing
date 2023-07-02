import { Component, Inject } from '@angular/core';
import { Solve } from '../../models/solve';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

export interface DialogData {
  solve: Solve;
  time: string;
}

@Component({
  selector: 'app-solve-info',
  templateUrl: './solve-info.component.html',
  styleUrls: ['./solve-info.component.css'],
})
export class SolveInfoComponent {
  public constructor(@Inject(MAT_DIALOG_DATA) public data: DialogData) {}
}
