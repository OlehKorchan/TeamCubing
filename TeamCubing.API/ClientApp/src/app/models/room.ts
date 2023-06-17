import { Solve } from './solve';

export interface Room {
  id: string;
  name: string;
  wasOnceConnectedUserNames: string[];
  connectedUserNames: string[];
  solves: Solve[];
}
