import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthenticationService } from '../../services/authentication.service';

@Component({
  template: '',
})
export class LogoutComponent implements OnInit {
  public constructor(
    private _authenticationService: AuthenticationService,
    private _router: Router,
  ) {}

  public ngOnInit(): void {
    this._authenticationService.logout();
    this._router.navigate(['']);
  }
}
