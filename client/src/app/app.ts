import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-root',
  imports: [],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {

  private http = inject(HttpClient);
  
  protected readonly title = signal('client');
  protected members = signal<any>([]);

  async ngOnInit(): Promise<void> {
    this.members.set(await this.getMembers())
  }

  async getMembers() {
    try {
      return lastValueFrom(this.http.get('https://localhost:5001/api/members'))
    }catch (e) {
      throw e;
    }
  }

}
