import { RoomPuzzle } from './roomSettings';

export interface RoomDisplayDataResponse {
  id: string;
  roomName: string;
  puzzle: RoomPuzzle;
  administratorName: string;
  connectedUsersCount: number;
  maxUsersCount: number;
  isOpen: boolean;
}
