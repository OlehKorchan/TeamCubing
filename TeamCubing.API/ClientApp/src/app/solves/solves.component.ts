import { Component, Input, OnInit, Output, ViewChild } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { MatTable } from '@angular/material/table';
import { Solve } from '../models/solve';
import { DialogService } from '../services/dialog.service';
import { SolveService } from '../services/solve.service';
import { MsToTimePipe } from '../pipes/ms-to-time.pipe';

@Component({
  selector: 'app-solves',
  templateUrl: './solves.component.html',
  styleUrls: ['./solves.component.css'],
})
export class SolvesComponent implements OnInit {
  @Input()
  public dataSource!: Solve[];

  @Input()
  public onNewSolve!: Observable<void>;

  public displayedColumns: string[] = ['position', 'time', 'action'];

  @ViewChild(MatTable)
  public table!: MatTable<Solve>;

  @Output()
  public solveDeleted: Subject<number> = new Subject<number>();

  public constructor(
    private dialogService: DialogService,
    private solveService: SolveService,
    private msToTimePipe: MsToTimePipe,
  ) {}

  public ngOnInit() {
    this.onNewSolve.subscribe({
      next: () => this.table.renderRows(),
    });
  }

  public removeSolve(id: number, time: number) {
    this.dialogService
      .showMessageDialog(
        'DELETE SOLVE',
        `Are you sure to delete solve ${this.msToTimePipe.transform(time)}`,
      )
      .afterClosed()
      .subscribe({
        next: (res: boolean) => {
          if (res) {
            this.solveService.removeSolve(id).subscribe({
              next: (result: boolean) => {
                if (result) {
                  this.solveDeleted.next(id);
                }
              },
            });
          }
        },
      });
  }
}
