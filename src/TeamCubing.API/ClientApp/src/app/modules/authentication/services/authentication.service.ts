import { Injectable } from '@angular/core';
import { Observable, shareReplay, tap } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import * as moment from 'moment';
import { ConfigurationService } from 'src/app/shared/services/configuration.service';
import { ILoginResponse } from 'src/app/modules/authentication/models/login-response';
import { ILoginRequest } from '../models/login-request';
import { IRegisterRequest } from '../models/register-request';
import { IRegisterResponse } from '../models/register-response';

@Injectable({
  providedIn: 'root',
})
export class AuthenticationService {
  public constructor(private _httpClient: HttpClient, private _config: ConfigurationService) {}

  public login(loginRequest: ILoginRequest): Observable<ILoginResponse> {
    return this._httpClient.post<ILoginResponse>(this._config.getLoginUrl(), loginRequest).pipe(
      tap((data) => this.setUserSession(data)),
      shareReplay(),
    );
  }

  public register(registerRequest: IRegisterRequest): Observable<IRegisterResponse> {
    return this._httpClient
      .post<IRegisterResponse>(this._config.getRegisterUrl(), registerRequest)
      .pipe(
        tap((data) => this.setUserSession(data)),
        shareReplay(),
      );
  }

  public logout() {
    localStorage.removeItem(this._config.getExpiresAtSessionKey());
    localStorage.removeItem(this._config.getUserTokenSessionKey());
    localStorage.removeItem(this._config.getUserNameSessionKey());
    console.log('User data has been deleted from client');
  }

  public getUserName() {
    let username = localStorage.getItem(this._config.getUserNameSessionKey());

    if (!username) {
      username = '';
    }

    return username;
  }

  public getToken(): string {
    return localStorage.getItem(this._config.getUserTokenSessionKey()) ?? '';
  }

  public isLoggedIn() {
    return moment().isBefore(this.getExpiration());
  }

  public isLoggedOut() {
    return !this.isLoggedIn();
  }

  public getExpiration() {
    const expiration = localStorage.getItem(this._config.getExpiresAtSessionKey());

    if (expiration === null) {
      return expiration;
    }

    const expiresAt = JSON.parse(expiration);

    return moment(expiresAt);
  }

  private setUserSession(userData: ILoginResponse | IRegisterResponse): void {
    const expiresAt = moment().add(userData.expiresIn, 'hour');

    localStorage.setItem(this._config.getUserTokenSessionKey(), userData.token);
    localStorage.setItem(this._config.getUserNameSessionKey(), userData.username);
    localStorage.setItem(
      this._config.getExpiresAtSessionKey(),
      JSON.stringify(expiresAt.valueOf()),
    );
  }
}
