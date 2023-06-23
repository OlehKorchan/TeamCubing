import { Color, PuzzleImage } from '../models/puzzles/puzzleImage';

export default class Utils {
  public static get threeByThreeSolvedImage(): PuzzleImage {
    return {
      faces: [
        {
          colors: [
            [Color.Red, Color.Red, Color.Red],
            [Color.Red, Color.Red, Color.Red],
            [Color.Red, Color.Red, Color.Red],
          ],
        },
        {
          colors: [
            [Color.White, Color.White, Color.White],
            [Color.White, Color.White, Color.White],
            [Color.White, Color.White, Color.White],
          ],
        },
        {
          colors: [
            [Color.Green, Color.Green, Color.Green],
            [Color.Green, Color.Green, Color.Green],
            [Color.Green, Color.Green, Color.Green],
          ],
        },
        {
          colors: [
            [Color.Orange, Color.Orange, Color.Orange],
            [Color.Orange, Color.Orange, Color.Orange],
            [Color.Orange, Color.Orange, Color.Orange],
          ],
        },
        {
          colors: [
            [Color.Yellow, Color.Yellow, Color.Yellow],
            [Color.Yellow, Color.Yellow, Color.Yellow],
            [Color.Yellow, Color.Yellow, Color.Yellow],
          ],
        },
        {
          colors: [
            [Color.Blue, Color.Blue, Color.Blue],
            [Color.Blue, Color.Blue, Color.Blue],
            [Color.Blue, Color.Blue, Color.Blue],
          ],
        },
      ],
    };
  }
}
