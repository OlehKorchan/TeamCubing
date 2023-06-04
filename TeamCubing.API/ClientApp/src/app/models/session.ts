import { Solve } from './solve';

export interface Session {
  id: number;
  name: string;
  solves: Solve[];
}
