import { IRoomSolve } from './roomSolve';

export interface IRoomLoginResult {
  result: boolean;
  roomId: number;
  connectedUserNames: string[];
  solves: IRoomSolve[];
}
