import { Component, OnInit } from '@angular/core';
import { UserService } from '../../services/user.service';
import { SolveService } from '../../services/solve.service';
import { Penalty } from '../../models/solve';
import { UserStatisticsResponse } from '../../models/userStatisticsResponse';
import { RoomPuzzle } from '../../models/roomSettings';
import { UserSolve } from '../../models/userSolve';

@Component({
  selector: 'app-personal-results',
  templateUrl: './personal-results.component.html',
  styleUrls: ['./personal-results.component.css'],
})
export class PersonalResultsComponent implements OnInit {
  public bestResultsDisplayedColumns: string[] = ['Event', 'Single', 'Average'];
  public allResultsDisplayedColumns: string[] = ['Time', 'Date', 'Room'];

  public Penalty = Penalty;

  public userResults: UserStatisticsResponse = {
    allResultsByPuzzles: [],
    bestResultsByPuzzle: [],
  };

  public constructor(private userService: UserService, public solveService: SolveService) {}

  public ngOnInit(): void {
    this.userService.getCurrentUserResults().subscribe({
      next: (results: UserStatisticsResponse) => {
        this.userResults = results;
      },
    });
  }

  public resultsByPuzzle(puzzle: RoomPuzzle): UserSolve[] {
    return this.userResults.allResultsByPuzzles.filter((r) => r.puzzle === puzzle);
  }
}
