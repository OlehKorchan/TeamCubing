import { Component, OnInit, ViewChild } from '@angular/core';
import { BreakpointObserver, BreakpointState } from '@angular/cdk/layout';
import { MatSidenav } from '@angular/material/sidenav';
import { Router } from '@angular/router';
import { AuthenticationService } from '../../../authentication/services/authentication.service';

const SMALL_WIDTH_BREAKPOINT = 720;

@Component({
  selector: 'app-sidenav',
  templateUrl: './sidenav.component.html',
  styleUrls: ['./sidenav.component.css'],
})
export class SidenavComponent implements OnInit {
  @ViewChild('sideNav')
  public sideNav: MatSidenav | undefined;
  public isScreenSmall = false;

  public constructor(
    private breakPointObserver: BreakpointObserver,
    private authService: AuthenticationService,
    private router: Router,
  ) {}

  public ngOnInit(): void {
    this.breakPointObserver
      .observe([`(max-width: ${SMALL_WIDTH_BREAKPOINT}px)`])
      .subscribe((state: BreakpointState) => {
        this.isScreenSmall = state.matches;
      });

    this.router.events.subscribe(() => {
      if (this.isScreenSmall) {
        this.sideNav?.close();
      }
    });
  }

  public get isUserLoggedIn() {
    return this.authService.isLoggedIn();
  }

  public toggleSidenav() {
    if (this.sideNav instanceof MatSidenav) {
      this.sideNav.toggle();
    }
  }
}
