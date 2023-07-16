import { RoomPuzzle } from './roomSettings';

export interface ChangePuzzleRequest {
  roomName: string;
  puzzle: RoomPuzzle;
}
