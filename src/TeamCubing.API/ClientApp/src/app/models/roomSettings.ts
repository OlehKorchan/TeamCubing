export interface RoomSettings {
  puzzle: RoomPuzzle;
  isOpen: boolean;
  enableSolveTimeLimit: boolean;
  usersLimit: number;
}

export enum RoomPuzzle {
  ThreeByThreeCube,
}
