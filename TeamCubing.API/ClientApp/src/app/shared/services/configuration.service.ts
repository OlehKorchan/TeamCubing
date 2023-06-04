import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ConfigurationService {
  public getApiUrl(): string {
    return environment.apiUrl;
  }

  public getLoginUrl(): string {
    return `${this.getApiUrl()}/account/login`;
  }

  public getRegisterUrl(): string {
    return `${this.getApiUrl()}/account/register`;
  }

  public getLogoutUrl(): string {
    return `${this.getApiUrl()}/account/logout`;
  }

  public getUserTokenSessionKey(): string {
    return 'user_token';
  }

  public getUserNameSessionKey(): string {
    return 'username';
  }

  public getExpiresAtSessionKey(): string {
    return 'expires_at';
  }
}
