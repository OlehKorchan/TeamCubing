import { Solve } from './solve';

export interface UserSolveResult {
  solve: Solve;
  timeColor: 'red' | 'green' | 'black';
  formattedTime: string;
}
