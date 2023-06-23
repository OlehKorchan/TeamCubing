import { BaseResponse } from './baseResponse';

export interface ModelResponse<T> extends BaseResponse {
  model: T;
}
