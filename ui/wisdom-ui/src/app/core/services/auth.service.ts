import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { map, tap, catchError } from 'rxjs/operators';
import {
  AuthResponse,
  SignInRequest,
  SignUpRequest,
  User,
  UserRoleEnum,
} from '../models/user.model';
import { EnvironmentService } from './environment.service';

const STORAGE_KEY = 'wisdom_user';
const TOKEN_KEY   = 'wisdom_token';

interface BffLoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: {
    id: string;
    email: string;
    name: string;
    role: string;
  };
}

function mapRole(role: string): UserRoleEnum {
  if (role === 'teacher') return UserRoleEnum.Teacher;
  return UserRoleEnum.Customer;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _http = inject(HttpClient);
  private readonly _env  = inject(EnvironmentService);

  private readonly _currentUser  = signal<User | null>(this._loadFromStorage());
  private readonly _accessToken  = signal<string | null>(this._loadToken());

  readonly currentUser     = this._currentUser.asReadonly();
  readonly accessToken     = this._accessToken.asReadonly();
  readonly isAuthenticated = computed(() => this._currentUser() !== null);
  readonly isTeacher       = computed(() => this._currentUser()?.role === UserRoleEnum.Teacher);
  readonly isStudent       = computed(() => this._currentUser()?.role === UserRoleEnum.Customer);

  signIn(request: SignInRequest): Observable<AuthResponse> {
    const url = `${this._env.apiUrl}/bff/auth/login`;
    return this._http
      .post<BffLoginResponse>(url, { email: request.email, password: request.password })
      .pipe(
        map((res) => {
          const user: User = {
            id:         res.user.id,
            keycloakId: res.user.id,
            email:      res.user.email,
            name:       res.user.name,
            role:       mapRole(res.user.role),
          };
          return {
            user,
            accessToken:  res.accessToken,
            refreshToken: res.refreshToken,
            expiresIn:    res.expiresIn,
          } satisfies AuthResponse;
        }),
        tap((response) => this._persist(response.user, response.accessToken)),
        catchError(() =>
          throwError(() => new Error('Invalid email or password. Please try again.')),
        ),
      );
  }

  signUp(_request: SignUpRequest): Observable<AuthResponse> {
    return throwError(() => new Error('Self-registration is not yet available. Please use a demo account.'));
  }

  logout(): void {
    this._currentUser.set(null);
    this._accessToken.set(null);
    localStorage.removeItem(STORAGE_KEY);
    localStorage.removeItem(TOKEN_KEY);
  }

  private _persist(user: User, token: string): void {
    this._currentUser.set(user);
    this._accessToken.set(token);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(user));
    localStorage.setItem(TOKEN_KEY, token);
  }

  private _loadFromStorage(): User | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? (JSON.parse(raw) as User) : null;
    } catch {
      return null;
    }
  }

  private _loadToken(): string | null {
    try {
      return localStorage.getItem(TOKEN_KEY);
    } catch {
      return null;
    }
  }
}

