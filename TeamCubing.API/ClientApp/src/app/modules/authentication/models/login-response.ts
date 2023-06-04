import { IResponse } from '../../../shared/models/response';

export interface ILoginResponse extends IResponse {
  username: string;
  token: string;
  expiresIn: number;
}
