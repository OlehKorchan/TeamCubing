import { RoomPuzzle } from './roomSettings';
import { Penalty } from './solve';

export interface UserSolve {
  roomName: string;
  scramble: string;
  puzzle: RoomPuzzle;
  time: number;
  penalty: Penalty;
  dateAdded: Date;
}
