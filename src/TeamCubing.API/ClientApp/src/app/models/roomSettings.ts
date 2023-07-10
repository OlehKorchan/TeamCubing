export interface RoomSettings {
  puzzle: RoomPuzzle;
  isOpen: boolean;
  enableSolveTimeLimit: boolean;
  usersLimit: number;
}

export enum RoomPuzzle {
  ThreeByThreeCube = 3,
  TwoByTwoCube = 2,
  FourByFourCube = 4,
  FiveByFiveCube = 5,
  SixBySixCube = 6,
  SevenBySevenCube = 7,
  Megaminx = 12,
}
