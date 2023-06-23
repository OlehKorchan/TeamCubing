import { ModelResponse } from './modelResponse';

export interface RoomOperationResponse<T> extends ModelResponse<T> {
  roomName: string;
}
