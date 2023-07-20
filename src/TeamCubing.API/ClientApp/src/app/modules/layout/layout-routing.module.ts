import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { RoomsComponent } from '../../components/rooms/rooms.component';
import { AuthenticationGuard } from '../authentication/guards/authentication.guard';
import { PersonalResultsComponent } from '../../components/personal-results/personal-results.component';

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
    path: 'results',
    component: PersonalResultsComponent,
    canActivate: [AuthenticationGuard],
  },
  {
    path: '',
    redirectTo: 'rooms',
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
