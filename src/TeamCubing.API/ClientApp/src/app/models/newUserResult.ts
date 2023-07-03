import { Penalty } from './solve';

export interface NewUserResult {
  roomId: string;
  solveNumber: number;
  timeInMilliseconds: number;
  penalty: Penalty;
}
