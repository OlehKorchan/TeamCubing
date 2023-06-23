import { RoomPuzzle } from './roomSettings';

export interface RoomDisplayDataResponse {
  roomName: string;
  puzzle: RoomPuzzle;
  connectedUsersCount: number;
  maxUsersCount: number;
  isOpen: boolean;
}
