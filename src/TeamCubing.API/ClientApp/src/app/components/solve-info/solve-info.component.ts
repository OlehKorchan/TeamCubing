import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

export interface DialogData {
  scramble: string;
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
