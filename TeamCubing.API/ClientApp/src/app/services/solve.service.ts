import { Injectable } from '@angular/core';
import { SolveResult } from '../models/solve';
import { Room } from '../models/room';
import { AuthenticationService } from '../modules/authentication/services/authentication.service';

@Injectable({
  providedIn: 'root',
})
export class SolveService {
  public constructor(private auth: AuthenticationService) {}

  public getCurrentUserResultsFromRoom(room: Room): SolveResult[] {
    const currentUser = this.auth.getUserName();

    return room.solves.flatMap((s) => s.results?.find((r) => r.userName === currentUser) ?? []);
  }

  public calculateAverage(averageOf: number, results: SolveResult[]): number {
    const size = results.length;
    if (size < averageOf) {
      return 0;
    }

    let minValue = 0;
    let maxValue = 0;
    let sum = 0;

    for (const result of results.slice(-averageOf)) {
      sum += result.time;

      if (result.time < minValue) {
        minValue = result.time;
      } else if (result.time > maxValue) {
        maxValue = result.time;
      }
    }

    sum -= minValue;
    sum -= maxValue;

    return sum / size;
  }

  public calculateMean(meanOf: number, results: SolveResult[]): number {
    const size = results.length;
    if (meanOf !== 0 && size < meanOf) {
      return 0;
    }

    let sum = 0;

    for (const result of results.slice(-meanOf)) {
      sum += result.time;
    }

    return sum / results.length;
  }
}
