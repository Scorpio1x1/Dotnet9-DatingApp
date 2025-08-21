import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class BusyService {
  busyRequestCount = signal(0);

  busy() {
    this.busyRequestCount.update((curr) => { 
      return curr + 1
    });
  }

  idle() {
    this.busyRequestCount.update(curr => { 
      return Math.max(0, curr - 1)
    })
  }
}
