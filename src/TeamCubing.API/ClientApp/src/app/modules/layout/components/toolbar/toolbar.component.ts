import { Component, Output, EventEmitter } from '@angular/core';
import { AuthenticationService } from '../../../authentication/services/authentication.service';

@Component({
  selector: 'app-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.css'],
})
export class ToolbarComponent {
  @Output()
  public toggleSidenav = new EventEmitter<void>();

  public constructor(private _authenticationService: AuthenticationService) {}

  public get isUserLoggedIn(): boolean {
    const state = this._authenticationService.isLoggedIn();

    return state;
  }

  public get userName(): string {
    return this._authenticationService.getUserName();
  }
}
