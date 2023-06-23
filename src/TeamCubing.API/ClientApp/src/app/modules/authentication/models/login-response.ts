import { BaseResponse } from '../../../models/baseResponse';

export interface ILoginResponse extends BaseResponse {
  username: string;
  token: string;
  expiresIn: number;
}
