import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../core/services/account-service';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastService } from '../../core/services/toast-service';

@Component({
  selector: 'app-nav',
  imports: [FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './nav.html',
  styleUrl: './nav.css'
})
export class Nav {
  protected accountService = inject(AccountService);
  private router = inject(Router);
  private toastr = inject(ToastService)

  protected creds: any = {};


  login() {
    this.accountService.login(this.creds).subscribe({
      next: _ => {
        this.router.navigateByUrl('/members');
        this.creds = {};
        this.toastr.success("Successful Login!")
      },
      error: error => {
        console.log(error);
        this.toastr.error(error.error);
      }
    });
  }

  logout() {
    this.router.navigateByUrl('/');
    this.accountService.logout();
    this.creds = {};
  }
}
