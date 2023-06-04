import { NgModule } from '@angular/core';

import { ErrorRoutingModule } from './error-routing.module';
import { NoAccessComponent } from './components/no-access/no-access.component';
import { NotFoundComponent } from './components/not-found/not-found.component';
import { ServerErrorComponent } from './components/server-error/server-error.component';

@NgModule({
  declarations: [NoAccessComponent, NotFoundComponent, ServerErrorComponent],
  imports: [ErrorRoutingModule],
})
export class ErrorModule {}
