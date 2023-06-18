import { Injectable } from '@angular/core';
import { Solve, SolveResult } from '../models/solve';
import { AuthenticationService } from '../modules/authentication/services/authentication.service';
import { ConfigurationService } from '../shared/services/configuration.service';
import { ReturnStatement } from '@angular/compiler';

@Injectable({
  providedIn: 'root',
})
export class SolveService {
  public constructor(private auth: AuthenticationService, private config: ConfigurationService) {}

  public calculateAverage(averageOf: number, solves: Solve[]): number {
    const avgResults = this.pullCurrentUserValidResults(averageOf, solves);

    const size = avgResults.length;
    if (size < averageOf) {
      return 0;
    }

    let minValue = avgResults[0].time;
    let maxValue = avgResults[0].time;
    let sum = 0;
    let count = 0;

    let dnfCount = 0;

    for (const result of avgResults) {
      sum += Math.abs(result.time);
      count++;

      if (result.time < 0) {
        dnfCount++;
        maxValue = result.time;
      } else if (result.time < minValue) {
        minValue = result.time;
      } else if (dnfCount === 0 && result.time > maxValue) {
        maxValue = result.time;
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
    const results = this.pullCurrentUserValidResults(meanOf, solves);
    const size = results.length;
    if (meanOf !== 0 && size < meanOf) {
      return 0;
    }

    let sum = 0;
    let validResultsLength = 0;

    for (const result of results) {
      if (result.time > 0) {
        sum += result.time;
        validResultsLength++;
      }
    }

    return sum / validResultsLength;
  }

  private pullCurrentUserValidResults(take: number, solves: Solve[]): SolveResult[] {
    const rightSolves = solves
      .flatMap((s) => (s.results?.find((r) => r.userName === this.auth.getUserName()) ? s : []))
      .sort((one, two) => (one.solveNumber < two.solveNumber ? -1 : 1))
      .slice(-take);

    return rightSolves.flatMap(
      (s) => s.results.find((r) => r.userName === this.auth.getUserName()) ?? [],
    );
  }
}
