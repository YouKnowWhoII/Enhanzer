import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse } from '../models/auth.models';

const tokenKey = 'enhanzer.auth.token';
const expiryKey = 'enhanzer.auth.expiry';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenSignal = signal<string | null>(sessionStorage.getItem(tokenKey));
  readonly isAuthenticated = computed(() => {
    const token = this.tokenSignal();
    const expiry = sessionStorage.getItem(expiryKey);
    return Boolean(token && expiry && new Date(expiry) > new Date());
  });

  constructor(private readonly http: HttpClient) {}

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiBaseUrl}/auth/login`, request).pipe(
      tap(response => {
        sessionStorage.setItem(tokenKey, response.token);
        sessionStorage.setItem(expiryKey, response.expiresAtUtc);
        this.tokenSignal.set(response.token);
      })
    );
  }

  getToken(): string | null {
    return this.isAuthenticated() ? this.tokenSignal() : null;
  }

  logout(): void {
    sessionStorage.removeItem(tokenKey);
    sessionStorage.removeItem(expiryKey);
    this.tokenSignal.set(null);
  }
}
