import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { AuthService } from './auth.service';
import { UserRoleEnum } from '../models/user.model';

describe('AuthService', () => {
  let service: AuthService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
    service = TestBed.inject(AuthService);
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should start with null currentUser when localStorage is empty', () => {
    expect(service.currentUser()).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
  });

  describe('signIn', () => {
    it('should return AuthResponse for a known email', fakeAsync(() => {
      let result: any;
      service.signIn({ email: 'teacher@wisdom.dev', password: 'any' }).subscribe((r) => (result = r));
      tick(1000);

      expect(result).toBeDefined();
      expect(result.user.email).toBe('teacher@wisdom.dev');
      expect(result.user.role).toBe(UserRoleEnum.Teacher);
      expect(result.accessToken).toContain('mock-access-token');
    }));

    it('should update currentUser signal after successful signIn', fakeAsync(() => {
      service.signIn({ email: 'teacher@wisdom.dev', password: 'any' }).subscribe();
      tick(1000);

      expect(service.currentUser()?.email).toBe('teacher@wisdom.dev');
      expect(service.isTeacher()).toBe(true);
    }));

    it('should throw an error for unknown email', fakeAsync(() => {
      let errorMsg = '';
      service.signIn({ email: 'unknown@example.com', password: 'any' }).subscribe({
        error: (e) => (errorMsg = e.message),
      });
      tick(1000);

      expect(errorMsg).toContain('Invalid email or password');
      expect(service.currentUser()).toBeNull();
    }));
  });

  describe('signUp', () => {
    it('should create a new user and update currentUser signal', fakeAsync(() => {
      let result: any;
      service
        .signUp({ email: 'new@wisdom.dev', name: 'New User', password: 'secret123', role: UserRoleEnum.Customer })
        .subscribe((r) => (result = r));
      tick(1000);

      expect(result.user.email).toBe('new@wisdom.dev');
      expect(result.user.role).toBe(UserRoleEnum.Customer);
      expect(service.currentUser()?.email).toBe('new@wisdom.dev');
    }));

    it('should throw error for duplicate email', fakeAsync(() => {
      let errorMsg = '';
      service
        .signUp({ email: 'teacher@wisdom.dev', name: 'Dup', password: 'pass', role: UserRoleEnum.Teacher })
        .subscribe({ error: (e) => (errorMsg = e.message) });
      tick(1000);

      expect(errorMsg).toContain('already exists');
    }));
  });

  describe('logout', () => {
    it('should clear currentUser signal and localStorage', fakeAsync(() => {
      service.signIn({ email: 'student@wisdom.dev', password: 'any' }).subscribe();
      tick(1000);

      expect(service.currentUser()).not.toBeNull();

      service.logout();

      expect(service.currentUser()).toBeNull();
      expect(service.isAuthenticated()).toBe(false);
      expect(localStorage.getItem('wisdom_user')).toBeNull();
    }));
  });
});
