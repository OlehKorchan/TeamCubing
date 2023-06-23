import { RoomLoginRequest } from './roomLoginRequest';
import { RoomSettings } from './roomSettings';

export interface RoomCreateRequest extends RoomLoginRequest {
  settings: RoomSettings;
}
