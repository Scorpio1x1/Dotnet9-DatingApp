import { HttpInterceptorFn } from '@angular/common/http';
import { catchError, switchMap, throwError } from 'rxjs';
import { ToastService } from '../services/toast-service';
import { inject } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';
import { AccountService } from '../services/account-service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toastr = inject(ToastService);
  const router = inject(Router);
  const accountService = inject(AccountService);
  return next(req).pipe(catchError(error => {
    if(error) {
      switch (error.status) {
        case 400:
          if(error.error.errors) {
            const modelStateErrors = [];
            for(const key in error.error.errors) {
              if(error.error.errors[key]) {
                modelStateErrors.push(error.error.errors[key])
              }
            }
            throw modelStateErrors.flat();
          } else {
            toastr.error(error.error);
          }
          break;
        case 401: {
          const isAuthEndpoint = req.url.includes('/account/login') ||
            req.url.includes('/account/register') ||
            req.url.includes('/account/refresh-token');
          const alreadyRetried = req.headers.has('X-Refresh-Attempt');

          if (isAuthEndpoint || alreadyRetried) {
            toastr.error('Unauthorized');
            break;
          }

          return accountService.refreshTokenWithState().pipe(
            switchMap(user => {
              if (!user) {
                accountService.logout();
                return throwError(() => error);
              }

              const retryReq = req.clone({
                setHeaders: {
                  Authorization: `Bearer ${user.token}`,
                  'X-Refresh-Attempt': '1'
                }
              });

              return next(retryReq);
            }),
            catchError(refreshError => {
              accountService.logout();
              toastr.error('Unauthorized');
              return throwError(() => refreshError);
            })
          );
        }

        case 404:
          router.navigateByUrl('/not-found')
          break;
        
        case 500:
          const navigationExtras: NavigationExtras = {state: {error: error.error}}
          router.navigateByUrl('/server-error', navigationExtras);
          break;
        default:
          toastr.error('Something went wrong');
          break;
      }
    }

    return throwError(() => error);
  }));
};
