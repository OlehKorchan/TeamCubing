import { PuzzleImage } from './puzzles/puzzleImage';

export interface Solve {
  solveNumber: number;
  results: SolveResult[];
  startTime: Date;
  scramble: string;
  scrambledPuzzleImage: PuzzleImage;
}

export interface SolveResult {
  userName: string;
  time: number;
  penalty: Penalty;
}

export enum Penalty {
  NoPenalty,
  PlusTwo,
  DNF,
}
