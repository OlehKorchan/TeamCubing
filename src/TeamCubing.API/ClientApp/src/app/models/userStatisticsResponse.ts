import { EventResult } from './eventResult';
import { UserSolve } from './userSolve';

export interface UserStatisticsResponse {
  bestResultsByPuzzle: EventResult[];
  allResultsByPuzzles: UserSolve[];
}
