import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthenticationService } from '../../services/authentication.service';

@Component({
  template: '',
})
export class LogoutComponent implements OnInit {
  constructor(private _authenticationService: AuthenticationService, private _router: Router) {}

  public ngOnInit(): void {
    this._authenticationService.logout().subscribe(() => this._router.navigate(['']));
  }
}
