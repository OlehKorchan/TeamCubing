import { Component, OnInit } from '@angular/core';
import { RoomCreateRequest } from '../../../models/roomCreateRequest';
import { RoomService } from '../../../services/room.service';
import { MatDialogRef } from '@angular/material/dialog';
import { ModelResponse } from '../../../models/modelResponse';
import { Room } from '../../../models/room';
import { RoomLoginRequest } from '../../../models/roomLoginRequest';
import { RoomPuzzle } from '../../../models/roomSettings';
import { SolveService } from '../../../services/solve.service';
import { FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-create-room-dialog',
  templateUrl: './create-room-dialog.component.html',
  styleUrls: ['./create-room-dialog.component.css'],
})
export class CreateRoomDialogComponent implements OnInit {
  public roomForm!: FormGroup;

  public response!: ModelResponse<Room>;

  public constructor(
    private roomService: RoomService,
    public dialogRef: MatDialogRef<CreateRoomDialogComponent>,
    public solveService: SolveService,
  ) {}

  public ngOnInit(): void {
    this.initForm();
  }

  public onSubmit(): void {
    if (this.roomForm.valid) {
      const request: RoomCreateRequest = {
        roomName: this.roomForm.get('name')?.value,
        roomPassword: this.roomForm.get('password')?.value,
        settings: {
          puzzle: this.roomForm.get('puzzle')?.value,
          isOpen: this.roomForm.get('isOpen')?.value,
          usersLimit: this.roomForm.get('maxUsers')?.value,
          enableSolveTimeLimit: this.roomForm.get('enableSolveDurationLimit')?.value,
        },
      };
      this.roomService.createRoom(request).subscribe({
        next: (response: ModelResponse<Room>): void => {
          if (response.isSuccess) {
            const loginRequest: RoomLoginRequest = {
              roomName: this.roomForm.get('name')?.value,
              roomPassword: this.roomForm.get('password')?.value,
            };
            this.dialogRef.close(loginRequest);
          } else {
            this.response = response;
          }
        },
      });
    }
  }

  private initForm(): void {
    this.roomForm = new FormGroup({
      puzzle: new FormControl(RoomPuzzle.ThreeByThreeCube),
      name: new FormControl('', [Validators.required]),
      isOpen: new FormControl(false),
      password: new FormControl(null),
      enableSolveDurationLimit: new FormControl(true),
      maxUsers: new FormControl(3, [Validators.required, Validators.max(5), Validators.min(1)]),
    });
  }
}
