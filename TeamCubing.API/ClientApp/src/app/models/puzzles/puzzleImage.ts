export interface PuzzleImage {
  faces: CubeFace[];
}

export interface CubeFace {
  colors: Color[][];
}

export enum Color {
  Red = 0,
  White = 1,
  Green = 2,
  Orange = 3,
  Yellow = 4,
  Blue = 5,
}
