import { Injectable } from '@angular/core';
import { BaseSolveResult, Penalty, Solve, SolveResult } from '../models/solve';
import { AuthenticationService } from '../modules/authentication/services/authentication.service';
import { ConfigurationService } from '../shared/services/configuration.service';
import { RoomPuzzle } from '../models/roomSettings';
import { SolveInfoComponent } from '../components/solve-info/solve-info.component';
import { MatDialog } from '@angular/material/dialog';
import { MsToTimePipe } from '../pipes/ms-to-time.pipe';

@Injectable({
  providedIn: 'root',
})
export class SolveService {
  private readonly emptyTime: string = '--:--';

  public availablePuzzles: RoomPuzzle[] = [
    RoomPuzzle.ThreeByThreeCube,
    RoomPuzzle.Megaminx,
    RoomPuzzle.TwoByTwoCube,
    RoomPuzzle.FourByFourCube,
    RoomPuzzle.FiveByFiveCube,
    RoomPuzzle.SixBySixCube,
    RoomPuzzle.SevenBySevenCube,
  ];

  public constructor(
    private auth: AuthenticationService,
    private config: ConfigurationService,
    private dialog: MatDialog,
  ) {}

  public formatResult(result: BaseSolveResult | undefined): string {
    if (result?.time) {
      const msToTimePipe = new MsToTimePipe();
      const isDnf = result.penalty === Penalty.DNF;

      return msToTimePipe.transform(result.time, isDnf);
    }

    return this.emptyTime;
  }

  public isBestResult(solve: Solve, result: SolveResult): boolean {
    return (
      result.time ===
      Math.min(
        ...solve.results.flatMap((r: SolveResult) => (r.penalty !== Penalty.DNF ? r.time : [])),
      )
    );
  }

  public calculateAverage(averageOf: number, solves: Solve[], user: string): number {
    const avgResults = this.pullUserValidResults(averageOf, solves, user);

    const size = avgResults.length;
    if (size < averageOf) {
      return 0;
    }

    let minValue = Number.MAX_VALUE;
    let maxValue = -1;
    let sum = 0;
    let count = 0;

    let dnfCount = 0;

    for (const result of avgResults) {
      sum += result.time;
      count++;

      if (result.penalty === Penalty.DNF) {
        dnfCount++;
        maxValue = result.time;
      } else {
        if (result.time < minValue) {
          minValue = result.time;
        }
        if (dnfCount === 0 && result.time > maxValue) {
          maxValue = result.time;
        }
      }
    }

    if (dnfCount > 1) {
      return this.config.dnfValue;
    }

    sum -= minValue;
    sum -= maxValue;

    return sum / (count - 2);
  }

  public calculateMean(meanOf: number, solves: Solve[], user: string): number {
    const results = this.pullUserValidResults(meanOf, solves, user);
    const size = results.length;
    if (meanOf !== 0 && size < meanOf) {
      return 0;
    }

    let sum = 0;
    let validResultsLength = 0;

    for (const result of results) {
      if (result.penalty !== Penalty.DNF) {
        sum += result.time;
        validResultsLength++;
      }
    }

    return sum / validResultsLength;
  }

  public bestUserSolve(user: string, solves: Solve[]): SolveResult {
    const results = this.getNonDnfUserResultsSorted(solves, user, 'ascending');

    if (results?.length) {
      return results[0];
    }

    return {
      time: 0,
      userName: user,
      penalty: Penalty.NoPenalty,
    };
  }

  public puzzleToString(puzzle: RoomPuzzle): string {
    switch (puzzle) {
      case RoomPuzzle.Megaminx:
        return 'Megaminx';
      case RoomPuzzle.TwoByTwoCube:
        return '2x2x2';
      case RoomPuzzle.FourByFourCube:
        return '4x4x4';
      case RoomPuzzle.FiveByFiveCube:
        return '5x5x5';
      case RoomPuzzle.SixBySixCube:
        return '6x6x6';
      case RoomPuzzle.SevenBySevenCube:
        return '7x7x7';
      case RoomPuzzle.ThreeByThreeCube:
        return '3x3x3';
    }
  }

  public getNonDnfUserResultsSorted(
    solves: Solve[],
    user: string,
    order: 'ascending' | 'descending',
  ): SolveResult[] {
    return solves
      ?.flatMap((s) => {
        return s.results?.find((r) => r.userName === user && r.penalty !== Penalty.DNF) ?? [];
      })
      ?.sort((one, two) => {
        if (order === 'ascending') {
          return one?.time > two?.time ? 1 : -1;
        }

        return one?.time > two?.time ? -1 : 1;
      });
  }

  public openSolveInfo(scramble: string, time: string): void {
    this.dialog.open(SolveInfoComponent, {
      data: {
        scramble: scramble,
        time: time,
      },
      width: '300px',
    });
  }

  private pullUserValidResults(take: number, solves: Solve[], user: string): SolveResult[] {
    return solves
      .slice()
      .sort((one, two) => (one.solveNumber < two.solveNumber ? -1 : 1))
      .flatMap((s) => s.results?.find((r) => r.userName === user) ?? [])
      .slice(-take);
  }
}
