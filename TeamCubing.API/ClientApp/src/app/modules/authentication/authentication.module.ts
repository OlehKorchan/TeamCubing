import { NgModule } from '@angular/core';
import { MaterialModule } from 'src/app/shared/material.module';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { LogoutComponent } from './components/logout/logout.component';
import { SharedModule } from '../../shared/shared.module';
import { AuthenticationRoutingModule } from './authentication-routing.module';

@NgModule({
  declarations: [LoginComponent, RegisterComponent, LogoutComponent],
  imports: [AuthenticationRoutingModule, SharedModule, MaterialModule],
  exports: [LogoutComponent],
})
export class AuthenticationModule {}
