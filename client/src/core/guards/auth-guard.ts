import { CanActivateFn } from '@angular/router';
import { AccountService } from '../services/account-service';
import { ToastService } from '../services/toast-service';
import { inject } from '@angular/core';

export const authGuard: CanActivateFn = (_, __) => {
  const accountService = inject(AccountService);
  const toastr = inject(ToastService);

  if (accountService.currentUser()) return true;
  else {
    toastr.error('You shall not pass!');
    return false;
  }
};
