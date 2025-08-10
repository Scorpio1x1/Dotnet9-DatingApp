import { Component, inject, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RegisterCreds, User } from '../../../types/user';
import { AccountService } from '../../../core/services/account-service';

@Component({
  selector: 'app-register',
  imports: [FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  private accountservice = inject(AccountService);
  cancelRegister = output<boolean>();
  protected creds = {} as RegisterCreds;


  register() {
    this.accountservice.register(this.creds).subscribe({
      next: r => {
        console.log(r);
        this.cancel();
      },
      error: e => {
        console.log("ERROR!!!! ", e);
      }
    })
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}
