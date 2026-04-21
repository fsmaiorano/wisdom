import { computed, Injectable, signal } from '@angular/core';
import { delay, Observable, of, throwError } from 'rxjs';
import { tap } from 'rxjs/operators';
import {
  AuthResponse,
  SignInRequest,
  SignUpRequest,
  User,
  UserRoleEnum,
} from '../models/user.model';

const STORAGE_KEY = 'wisdom_user';

const MOCK_USERS: User[] = [
  {
    id: '11111111-0000-0000-0000-000000000001',
    keycloakId: 'kc-teacher-1',
    email: 'teacher@wisdom.dev',
    name: 'Alice Teacher',
    role: UserRoleEnum.Teacher,
  },
  {
    id: '22222222-0000-0000-0000-000000000002',
    keycloakId: 'kc-student-1',
    email: 'customer@wisdom.dev',
    name: 'Bob Student',
    role: UserRoleEnum.Customer,
  },
];

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _currentUser = signal<User | null>(this._loadFromStorage());
  readonly currentUser = this._currentUser.asReadonly();
  readonly isAuthenticated = computed(() => this._currentUser() !== null);
  readonly isTeacher = computed(() => this._currentUser()?.role === UserRoleEnum.Teacher);
  readonly isStudent = computed(() => this._currentUser()?.role === UserRoleEnum.Customer);

  signIn(request: SignInRequest): Observable<AuthResponse> {
    const found = MOCK_USERS.find((u) => u.email === request.email);

    if (!found) {
      return throwError(() => new Error('Invalid email or password.')).pipe(delay(500));
    }

    const response = this._buildAuthResponse(found);
    return of(response).pipe(
      delay(600),
      tap(() => this._persist(found)),
    );
  }

  signUp(request: SignUpRequest): Observable<AuthResponse> {
    const exists = MOCK_USERS.some((u) => u.email === request.email);

    if (exists) {
      return throwError(() => new Error('An account with this email already exists.')).pipe(
        delay(500),
      );
    }

    const newUser: User = {
      id: crypto.randomUUID(),
      keycloakId: `kc-${crypto.randomUUID()}`,
      email: request.email,
      name: request.name,
      role: request.role,
    };

    MOCK_USERS.push(newUser);
    const response = this._buildAuthResponse(newUser);

    return of(response).pipe(
      delay(800),
      tap(() => this._persist(newUser)),
    );
  }

  logout(): void {
    this._currentUser.set(null);
    localStorage.removeItem(STORAGE_KEY);
  }

  private _buildAuthResponse(user: User): AuthResponse {
    return {
      user,
      accessToken: `mock-access-token-${user.id}`,
      refreshToken: `mock-refresh-token-${user.id}`,
      expiresIn: 3600,
    };
  }

  private _persist(user: User): void {
    this._currentUser.set(user);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(user));
  }

  private _loadFromStorage(): User | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? (JSON.parse(raw) as User) : null;
    } catch {
      return null;
    }
  }
}
