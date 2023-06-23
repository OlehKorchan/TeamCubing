import { Injectable } from '@angular/core';
import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import { catchError, finalize, Observable, throwError } from 'rxjs';
import { NgxSpinnerService } from 'ngx-spinner';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ConfigurationService } from '../services/configuration.service';

@Injectable()
export class GlobalInterceptor implements HttpInterceptor {
  public constructor(
    private _config: ConfigurationService,
    private spinner: NgxSpinnerService,
    private _router: Router,
    private _snackBar: MatSnackBar,
  ) {}

  public intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    this.spinner.show();
    const userToken = localStorage.getItem(this._config.getUserTokenSessionKey());

    if (userToken) {
      const cloned = request.clone({
        headers: request.headers.set('Authorization', `Bearer ${userToken}`),
      });

      return next.handle(cloned).pipe(
        catchError((error) => this.handleHttpError(error)),
        finalize(() => {
          this.spinner.hide();
        }),
      );
    }

    return next.handle(request).pipe(
      finalize(() => {
        this.spinner.hide();
      }),
    );
  }

  private handleHttpError(err: any) {
    if (err instanceof HttpErrorResponse) {
      if (err.error instanceof ErrorEvent) {
        console.error('Client side error', JSON.stringify(err.error.message));

        this._snackBar.open('Unexpected client error', 'close');
      } else {
        console.log(`Server error status : ${err.status} ${err.statusText}`);

        switch (err.status) {
          case 401:
            this._router.navigate(['/login']);
            break;
          case 403:
            this._router.navigate(['/error/no-access']);
            break;
          case 404:
            this._router.navigate(['/error/not-found']);
            break;
          case 500:
            this._router.navigate(['/error/server-error']);
            break;
          case 0:
            this._snackBar.open('Server down', 'close');
            break;
          default:
            this._snackBar.open('Server error', 'close');
        }
      }
    } else {
      console.error('Unexpected error');

      this._snackBar.open('Unexpected error', 'close');
    }

    return throwError(() => err);
  }
}
