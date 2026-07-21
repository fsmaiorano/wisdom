import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth.service';
import { UserRoleEnum } from '../models/user.model';

const BFF_LOGIN_URL = 'http://localhost:5001/bff/auth/login';

const TEACHER_RESPONSE = {
  accessToken: 'eyJhbGciOiJSUzI1NiJ9.eyJ1c2VySWQiOiIxMTExMTExMS0wMDAwLTAwMDAtMDAwMC0wMDAwMDAwMDAwMDEiLCJzdWIiOiIxMTExMTExMS0wMDAwLTAwMDAtMDAwMC0wMDAwMDAwMDAwMDEiLCJlbWFpbCI6InRlYWNoZXJAd2lzZG9tLmRldiIsIm5hbWUiOiJUZWFjaGVyIERlbW8iLCJyZWFsbV9hY2Nlc3MiOnsicm9sZXMiOlsidGVhY2hlciJdfX0.sig',
  refreshToken: 'refresh-teacher',
  expiresIn: 3600,
  user: { id: '11111111-0000-0000-0000-000000000001', email: 'teacher@wisdom.dev', name: 'Teacher Demo', role: 'teacher' },
};

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service  = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
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
    it('should return AuthResponse and update currentUser on success', () => {
      let result: any;
      service.signIn({ email: 'teacher@wisdom.dev', password: 'demoteacher' }).subscribe((r) => (result = r));

      const req = httpMock.expectOne(BFF_LOGIN_URL);
      expect(req.request.method).toBe('POST');
      req.flush(TEACHER_RESPONSE);

      expect(result).toBeDefined();
      expect(result.user.email).toBe('teacher@wisdom.dev');
      expect(result.user.role).toBe(UserRoleEnum.Teacher);
      expect(result.accessToken).toBe(TEACHER_RESPONSE.accessToken);
    });

    it('should update currentUser signal and persist token after signIn', () => {
      service.signIn({ email: 'teacher@wisdom.dev', password: 'demoteacher' }).subscribe();
      httpMock.expectOne(BFF_LOGIN_URL).flush(TEACHER_RESPONSE);

      expect(service.currentUser()?.email).toBe('teacher@wisdom.dev');
      expect(service.isTeacher()).toBe(true);
      expect(localStorage.getItem('wisdom_token')).toBe(TEACHER_RESPONSE.accessToken);
    });

    it('should emit error on 401 response', () => {
      let errorMsg = '';
      service.signIn({ email: 'wrong@example.com', password: 'wrongpass' }).subscribe({
        error: (e) => (errorMsg = e.message),
      });

      httpMock.expectOne(BFF_LOGIN_URL).flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });

      expect(errorMsg).toContain('Invalid email or password');
      expect(service.currentUser()).toBeNull();
    });
  });

  describe('signUp', () => {
    it('should return an error (registration not available)', () => {
      let errorMsg = '';
      service
        .signUp({ email: 'new@wisdom.dev', name: 'New', password: 'secret123', role: UserRoleEnum.Customer })
        .subscribe({ error: (e) => (errorMsg = e.message) });

      expect(errorMsg).toContain('not yet available');
    });
  });

  describe('logout', () => {
    it('should clear currentUser signal and localStorage', () => {
      service.signIn({ email: 'teacher@wisdom.dev', password: 'demoteacher' }).subscribe();
      httpMock.expectOne(BFF_LOGIN_URL).flush(TEACHER_RESPONSE);
      expect(service.currentUser()).not.toBeNull();

      service.logout();

      expect(service.currentUser()).toBeNull();
      expect(service.isAuthenticated()).toBe(false);
      expect(localStorage.getItem('wisdom_user')).toBeNull();
    });
  });
});

