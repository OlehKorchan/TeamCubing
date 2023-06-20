import { Solve } from './solve';
import { RoomSettings } from './roomSettings';

export interface Room {
  id: string;
  name: string;
  wasOnceConnectedUserNames: string[];
  connectedUserNames: string[];
  settings: RoomSettings;
  solves: Solve[];
}
