import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { NoAccessComponent } from './components/no-access/no-access.component';
import { NotFoundComponent } from './components/not-found/not-found.component';
import { ServerErrorComponent } from './components/server-error/server-error.component';

const routes: Routes = [
  { path: 'error/not-found', component: NotFoundComponent },
  { path: 'error/server-error', component: ServerErrorComponent },
  { path: 'error/no-access', component: NoAccessComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ErrorRoutingModule {}
