export interface IRoomSolve {
  id: number;
  solveNumber: number;
  results: ISolveResult[];
  roomId: number;
  scramble: string;
}

export interface ISolveResult {
  id: number;
  roomSolveId: number;
  userName: string;
  time: number;
}
