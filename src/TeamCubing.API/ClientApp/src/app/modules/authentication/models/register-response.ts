import { BaseResponse } from '../../../models/baseResponse';

export interface IRegisterResponse extends BaseResponse {
  username: string;
  token: string;
  expiresIn: number;
}
