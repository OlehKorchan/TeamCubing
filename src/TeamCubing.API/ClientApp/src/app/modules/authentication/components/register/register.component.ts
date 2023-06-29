import { Component, OnDestroy } from '@angular/core';
import { NgModel } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { IRegisterRequest } from '../../models/register-request';
import { IRegisterResponse } from '../../models/register-response';
import { AuthenticationService } from '../../services/authentication.service';

@Component({
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent implements OnDestroy {
  public registerRequest: IRegisterRequest = {
    userName: '',
    password: '',
    passwordConfirm: '',
  };
  public registerResultSubscription!: Subscription;
  public registerResponse: IRegisterResponse = {
    username: '',
    token: '',
    isSuccess: true,
    errorMessage: '',
    expiresIn: 0,
  };
  public hasErrors = false;
  public errorMessages: string[] = [];

  public constructor(
    private _authenticationService: AuthenticationService,
    private _router: Router,
  ) {}

  public onSubmit(): void {
    this.hasErrors = false;
    this.errorMessages = [];

    this.registerResultSubscription = this._authenticationService
      .register(this.registerRequest)
      .subscribe({
        next: (data) => {
          this.registerResponse = data;

          if (
            this.registerResponse.isSuccess &&
            this.registerResponse.token &&
            this.registerResponse.username
          ) {
            console.log(`User with username ${this.registerResponse.username} logged in`);
            this._router.navigate(['']);
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

  public validateConfirmPassword(passwordField: NgModel, confirmPasswordField: NgModel) {
    if (confirmPasswordField.value !== passwordField.value) {
      confirmPasswordField.control.setErrors({ duplicate: true });
    }
  }

  public ngOnDestroy(): void {
    if (this.registerResultSubscription) {
      this.registerResultSubscription.unsubscribe();
    }
  }
}
