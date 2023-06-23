import { Component, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router';
import { ILoginResponse } from 'src/app/modules/authentication/models/login-response';
import { AuthenticationService } from '../../services/authentication.service';
import { ILoginRequest } from '../../models/login-request';

@Component({
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnDestroy {
  public loginRequest: ILoginRequest = {
    login: '',
    password: '',
  };
  public loginResultSubscription!: Subscription;
  public loginResponse: ILoginResponse = {
    username: '',
    token: '',
    errorMessage: '',
    isSuccess: true,
    expiresIn: 0,
  };
  public hasErrors = false;
  public errorMessages: string[] = [];

  public constructor(
    private _authenticationService: AuthenticationService,
    private router: Router,
  ) {}

  public onSubmit(): void {
    this.errorMessages = [];

    this.loginResultSubscription = this._authenticationService.login(this.loginRequest).subscribe({
      next: (data) => {
        this.loginResponse = data;

        if (
          this.loginResponse.isSuccess &&
          this.loginResponse.token &&
          this.loginResponse.username
        ) {
          console.log(`User with username ${this.loginResponse.username} logged in`);
          this.router.navigate(['']);
        }
      },
      error: (error) => this.handleHttpError(error),
    });
  }

  public handleHttpError(error: any) {
    console.log(error?.error?.errorMessage);
    this.hasErrors = true;
    this.errorMessages.push(error?.error?.errorMessage);
  }

  public ngOnDestroy(): void {
    if (this.loginResultSubscription) {
      this.loginResultSubscription.unsubscribe();
    }
  }
}
