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
import { TimerComponent } from './components/timer/timer.component';
import { MsToTimePipe } from './pipes/ms-to-time.pipe';
import { DialogComponent } from './components/dialog/dialog.component';
import { RoomsComponent } from './components/rooms/rooms.component';
import { RouterModule } from '@angular/router';
import { CreateRoomDialogComponent } from './components/rooms/create-room-dialog/create-room-dialog.component';
import { MatTooltipModule } from '@angular/material/tooltip';
import { JoinRoomDialogComponent } from './components/rooms/join-room-dialog/join-room-dialog.component';
import { CubeImageComponent } from './components/cube-image/cube-image.component';
import { InputTimePipe } from './pipes/input-time.pipe';
import { InputTimeDirective } from './directives/input-time.directive';
import { SolveInfoComponent } from './components/solve-info/solve-info.component';
import { ConnectionErrorDialogComponent } from './components/connection-error-dialog/connection-error-dialog.component';
import { ChangePuzzleVerificationDialogComponent } from './components/rooms/change-puzzle-verification-dialog/change-puzzle-verification-dialog.component';
import { PersonalResultsComponent } from './components/personal-results/personal-results.component';
import { MatExpansionModule } from '@angular/material/expansion';

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
    InputTimePipe,
    InputTimeDirective,
    SolveInfoComponent,
    ConnectionErrorDialogComponent,
    ChangePuzzleVerificationDialogComponent,
    PersonalResultsComponent,
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
    MatExpansionModule,
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
