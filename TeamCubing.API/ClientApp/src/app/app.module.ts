import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { NgxSpinnerModule } from 'ngx-spinner';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AppComponent } from './app.component';
import { SharedModule } from './shared/shared.module';
import { GlobalInterceptor } from './shared/interceptors/global.interceptor';
import { LayoutModule } from './modules/layout/layout.module';
import { SolvesComponent } from './solves/solves.component';
import { MaterialModule } from './shared/material.module';
import { TimerComponent } from './timer/timer.component';
import { ScrambleComponent } from './scramble/scramble.component';
import { MsToTimePipe } from './pipes/ms-to-time.pipe';
import { SessionsComponent } from './sessions/sessions.component';
import { DialogComponent } from './dialog/dialog.component';
import { RoomsComponent } from './rooms/rooms.component';
import { RouterModule } from '@angular/router';

@NgModule({
  declarations: [
    AppComponent,
    SolvesComponent,
    TimerComponent,
    ScrambleComponent,
    MsToTimePipe,
    SessionsComponent,
    DialogComponent,
    RoomsComponent,
  ],
  imports: [
    SharedModule,
    MaterialModule,
    BrowserModule,
    HttpClientModule,
    NgxSpinnerModule,
    BrowserAnimationsModule,
    LayoutModule,
    RouterModule,
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: GlobalInterceptor,
      multi: true,
    },
  ],
  exports: [SolvesComponent],
  bootstrap: [AppComponent],
})
export class AppModule {}
