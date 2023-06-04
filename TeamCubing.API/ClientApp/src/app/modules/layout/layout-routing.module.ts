import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TimerComponent } from '../../timer/timer.component';
import { RoomsComponent } from '../../rooms/rooms.component';
import { AuthenticationGuard } from '../authentication/guards/authentication.guard';

const routes: Routes = [
  {
    path: 'error',
    loadChildren: () => import('../error/error.module').then((imp) => imp.ErrorModule),
  },
  {
    path: 'rooms/:roomName',
    component: RoomsComponent,
    canActivate: [AuthenticationGuard],
  },
  {
    path: 'rooms',
    component: RoomsComponent,
    canActivate: [AuthenticationGuard],
  },
  {
    path: '',
    component: TimerComponent,
    pathMatch: 'full',
  },
  {
    path: '**',
    redirectTo: 'error/not-found',
    pathMatch: 'full',
  },
];

@NgModule({
  declarations: [],
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class LayoutRoutingModule {}
