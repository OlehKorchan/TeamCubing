import { Injectable } from '@angular/core';
import { Penalty, Solve, SolveResult } from '../models/solve';
import { AuthenticationService } from '../modules/authentication/services/authentication.service';
import { ConfigurationService } from '../shared/services/configuration.service';
import { RoomPuzzle } from '../models/roomSettings';

@Injectable({
  providedIn: 'root',
})
export class SolveService {
  public constructor(private auth: AuthenticationService, private config: ConfigurationService) {}

  public calculateAverage(averageOf: number, solves: Solve[]): number {
    const avgResults = this.pullUserValidResults(averageOf, solves, this.auth.getUserName());

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

  public calculateMean(meanOf: number, solves: Solve[]): number {
    const results = this.pullUserValidResults(meanOf, solves, this.auth.getUserName());
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

  private pullUserValidResults(take: number, solves: Solve[], user: string): SolveResult[] {
    return solves
      .slice()
      .sort((one, two) => (one.solveNumber < two.solveNumber ? -1 : 1))
      .flatMap((s) => s.results?.find((r) => r.userName === user) ?? [])
      .slice(-take);
  }
}
