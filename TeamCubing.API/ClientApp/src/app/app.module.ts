import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { NgxSpinnerModule } from 'ngx-spinner';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AppComponent } from './app.component';
import { SharedModule } from './shared/shared.module';
import { GlobalInterceptor } from './shared/interceptors/global.interceptor';
import { LayoutModule } from './modules/layout/layout.module';
import { MaterialModule } from './shared/material.module';
import { TimerComponent } from './timer/timer.component';
import { MsToTimePipe } from './pipes/ms-to-time.pipe';
import { DialogComponent } from './dialog/dialog.component';
import { RoomsComponent } from './rooms/rooms.component';
import { RouterModule } from '@angular/router';
import { CreateRoomDialogComponent } from './rooms/create-room-dialog/create-room-dialog.component';
import { MatTooltipModule } from '@angular/material/tooltip';
import { JoinRoomDialogComponent } from './rooms/join-room-dialog/join-room-dialog.component';
import { CubeImageComponent } from './cube-image/cube-image.component';

@NgModule({
  declarations: [
    AppComponent,
    TimerComponent,
    MsToTimePipe,
    DialogComponent,
    RoomsComponent,
    CreateRoomDialogComponent,
    JoinRoomDialogComponent,
    CubeImageComponent,
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
    MatTooltipModule,
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: GlobalInterceptor,
      multi: true,
    },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
