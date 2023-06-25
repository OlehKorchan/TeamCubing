import { Component, Input } from '@angular/core';
import { Color, PuzzleImage } from '../../models/puzzles/puzzleImage';
import { RoomPuzzle } from '../../models/roomSettings';

@Component({
  selector: 'app-cube-image',
  templateUrl: './cube-image.component.html',
  styleUrls: ['./cube-image.component.css'],
})
export class CubeImageComponent {
  @Input()
  public scrambledCube!: PuzzleImage;

  @Input()
  public puzzleType: RoomPuzzle = RoomPuzzle.ThreeByThreeCube;

  public faceWidth = 60;
  public pieceWidth = 0.9;

  public get cubeSize(): number {
    switch (this.puzzleType) {
      case RoomPuzzle.ThreeByThreeCube:
        return 3;
    }
  }

  public getRGBColor(color: Color): string {
    switch (color) {
      case Color.White: {
        return 'rgb(255, 255, 255)';
      }
      case Color.Green: {
        return 'rgb(0, 255, 0)';
      }
      case Color.Red: {
        return 'rgb(255, 0, 0)';
      }
      case Color.Orange: {
        return 'rgb(255, 165, 0)';
      }
      case Color.Yellow: {
        return 'rgb(255, 255, 0)';
      }
      case Color.Blue: {
        return 'rgb(0, 0, 255)';
      }
    }
  }

  public getFaceXByIndex(index: number): number {
    if (index === 0) {
      return 6;
    } else if (index === 1) {
      return 3;
    } else if (index === 2) {
      return 3;
    } else if (index === 3) {
      return 0;
    } else if (index === 4) {
      return 3;
    } else if (index === 5) {
      return 9;
    }

    return 0;
  }

  public getFaceYByIndex(index: number): number {
    if (index === 0) {
      return 3;
    } else if (index === 1) {
      return 0;
    } else if (index === 2) {
      return 3;
    } else if (index === 3) {
      return 3;
    } else if (index === 4) {
      return 6;
    } else if (index === 5) {
      return 3;
    }

    return 0;
  }
}
