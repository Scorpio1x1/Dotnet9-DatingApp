import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { LoginCreds, RegisterCreds, User } from '../../types/user';
import { finalize, Observable, shareReplay, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { LikesService } from './likes-service';
import { PresenceService } from './presence-service';
import { HubConnectionState } from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private http = inject(HttpClient);
  private likesService = inject(LikesService);
  private presenceService = inject(PresenceService);
  currentUser = signal<User | null>(null);
  private baseUrl = environment.apiUrl;
  private refreshTimeoutId: number | null = null;
  private refreshInFlight?: Observable<User | null>;

  register(creds: RegisterCreds) {
    return this.http.post<User>(this.baseUrl + 'account/register', creds,
      { withCredentials: true }).pipe(
        tap(user => {
          if (user) {
            this.setCurrentUser(user);
          }
        })
      )
  }

  login(creds: LoginCreds) {
    return this.http.post<User>(this.baseUrl + 'account/login', creds,
      { withCredentials: true }).pipe(
        tap(user => {
          if (user) {
            this.setCurrentUser(user);
          }
        })
      )
  }

  refreshToken() {
    return this.http.post<User | null>(this.baseUrl + 'account/refresh-token', {},
      { withCredentials: true })
  }

  refreshTokenWithState() {
    if (this.refreshInFlight) return this.refreshInFlight;

    this.refreshInFlight = this.refreshToken().pipe(
      tap(user => {
        if (user) {
          this.setCurrentUser(user);
        }
      }),
      finalize(() => {
        this.refreshInFlight = undefined;
      }),
      shareReplay(1)
    );

    return this.refreshInFlight;
  }

  setCurrentUser(user: User) {
    user.roles = this.getRolesFromToken(user);
    this.currentUser.set(user);
    this.scheduleTokenRefresh(user.token);
    this.likesService.getLikeIds();
    if (this.presenceService.hubConnection?.state !== HubConnectionState.Connected) {
      this.presenceService.createHubConnection(() => this.currentUser()?.token)
    }
  }

  logout() {
    this.http.post(this.baseUrl + 'account/logout', {}, { withCredentials: true }).subscribe({
      next: () => {
        this.clearTokenRefreshTimer();
        localStorage.removeItem('filters');
        this.likesService.clearLikeIds();
        this.currentUser.set(null);
        this.presenceService.stopHubConnection();
      }
    })

  }

  private getRolesFromToken(user: User): string[] {
    const payload = user.token.split('.')[1];
    const decoded = atob(payload);
    const jsonPayload = JSON.parse(decoded);
    return Array.isArray(jsonPayload.role) ? jsonPayload.role : [jsonPayload.role]
  }

  private scheduleTokenRefresh(token: string) {
    this.clearTokenRefreshTimer();
    const delayMs = this.getRefreshDelayMs(token);

    this.refreshTimeoutId = window.setTimeout(() => {
      this.refreshTokenWithState().subscribe({
        error: () => this.logout()
      });
    }, delayMs);
  }

  private clearTokenRefreshTimer() {
    if (this.refreshTimeoutId !== null) {
      clearTimeout(this.refreshTimeoutId);
      this.refreshTimeoutId = null;
    }
  }

  private getRefreshDelayMs(token: string) {
    try {
      const payload = token.split('.')[1];
      const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
      const padded = base64.padEnd(base64.length + (4 - base64.length % 4) % 4, '=');
      const decoded = atob(padded);
      const jsonPayload = JSON.parse(decoded);
      const expMs = (jsonPayload.exp as number) * 1000;
      const refreshAt = expMs - 60_000;
      return Math.max(refreshAt - Date.now(), 0);
    } catch {
      return 0;
    }
  }
}