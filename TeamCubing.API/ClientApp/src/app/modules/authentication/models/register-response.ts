import { IResponse } from 'src/app/shared/models/response';

export interface IRegisterResponse extends IResponse {
  username: string;
  token: string;
  expiresIn: number;
}
