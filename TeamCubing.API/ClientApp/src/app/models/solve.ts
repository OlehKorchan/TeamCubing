export interface Solve {
  solveNumber: number;
  results: SolveResult[];
  startTime: Date;
  scramble: string;
}

export interface SolveResult {
  userName: string;
  time: number;
}
