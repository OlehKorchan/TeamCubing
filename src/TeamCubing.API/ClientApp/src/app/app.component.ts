import { Component } from '@angular/core';
import {
  NavigationCancel,
  NavigationEnd,
  NavigationError,
  NavigationStart,
  Router,
} from '@angular/router';
import { NgxSpinnerService } from 'ngx-spinner';
import { AuthenticationService } from './modules/authentication/services/authentication.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent {
  title = 'easy-tracker';
  constructor(
    private authService: AuthenticationService,
    router: Router,
    private spinner: NgxSpinnerService,
  ) {
    router.events.subscribe((e) => {
      this.navigationInterceptor(e);
    });
  }

  get isUserLoggedIn() {
    return this.authService.isLoggedIn();
  }

  navigationInterceptor(event: any): void {
    if (event instanceof NavigationStart) {
      this.spinner.show();
    } else if (event instanceof NavigationEnd) {
      this.spinner.hide();
    } else if (event instanceof NavigationCancel) {
      this.spinner.hide();
    } else if (event instanceof NavigationError) {
      this.spinner.hide();
    }
  }
}
