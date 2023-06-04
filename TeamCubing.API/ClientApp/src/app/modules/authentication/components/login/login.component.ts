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
  loginRequest: ILoginRequest = {
    login: '',
    password: '',
  };
  loginResultSubscription!: Subscription;
  loginResponse: ILoginResponse = {
    username: '',
    token: '',
    errors: [],
    expiresIn: 0,
  };
  hasErrors = false;
  errorMessages: string[] = [];

  constructor(private _authenticationService: AuthenticationService, private router: Router) {}

  onSubmit(): void {
    this.errorMessages = [];

    this.loginResultSubscription = this._authenticationService.login(this.loginRequest).subscribe({
      next: (data) => {
        this.loginResponse = data;

        if (
          !this.loginResponse.errors?.length &&
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

  handleHttpError(error: any) {
    console.log(error?.error?.errorMessage);
    this.hasErrors = true;
    this.errorMessages.push(error?.error?.errorMessage);
  }

  ngOnDestroy(): void {
    if (this.loginResultSubscription) {
      this.loginResultSubscription.unsubscribe();
    }
  }
}
