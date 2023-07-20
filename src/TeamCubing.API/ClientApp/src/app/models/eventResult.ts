import { RoomPuzzle } from './roomSettings';
import { BaseSolveResult } from './solve';

export interface EventResult {
  event: RoomPuzzle;
  single: BaseSolveResult;
  average: BaseSolveResult;
}
