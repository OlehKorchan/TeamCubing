import { NgModule } from '@angular/core';
import { SharedModule } from 'src/app/shared/shared.module';
import { MaterialModule } from 'src/app/shared/material.module';
import { ErrorModule } from '../error/error.module';

import { SidenavComponent } from './components/sidenav/sidenav.component';
import { ToolbarComponent } from './components/toolbar/toolbar.component';
import { LayoutRoutingModule } from './layout-routing.module';
import { AuthenticationModule } from '../authentication/authentication.module';

@NgModule({
  declarations: [SidenavComponent, ToolbarComponent],
  imports: [MaterialModule, SharedModule, AuthenticationModule, ErrorModule, LayoutRoutingModule],
  exports: [SidenavComponent],
})
export class LayoutModule {}
