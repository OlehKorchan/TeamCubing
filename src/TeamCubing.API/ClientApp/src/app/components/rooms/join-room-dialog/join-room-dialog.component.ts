import { Component, Inject } from '@angular/core';
import { RoomLoginRequest } from '../../../models/roomLoginRequest';
import { ModelResponse } from '../../../models/modelResponse';
import { Room } from '../../../models/room';
import { RoomService } from '../../../services/room.service';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

export interface DialogData {
  roomName: string;
}

@Component({
  selector: 'app-join-room-dialog',
  templateUrl: './join-room-dialog.component.html',
  styleUrls: ['./join-room-dialog.component.css'],
})
export class JoinRoomDialogComponent {
  public request: RoomLoginRequest = {
    roomName: '',
    roomPassword: undefined,
  };

  public response: ModelResponse<Room> | undefined;

  public constructor(
    private roomService: RoomService,
    public dialogRef: MatDialogRef<JoinRoomDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
  ) {
    this.request.roomName = data.roomName;
  }

  public onSubmit(): void {
    this.roomService.loginToRoom(this.request).subscribe({
      next: (response: ModelResponse<Room>): void => {
        if (response.isSuccess) {
          this.dialogRef.close(response.model);
        } else {
          this.response = response;
        }
      },
    });
  }
}
