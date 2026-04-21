import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { of, throwError } from 'rxjs';
import { SignUpComponent } from './sign-up.component';
import { AuthService } from '../../../core/services/auth.service';
import { User, UserRoleEnum } from '../../../core/models/user.model';

const MOCK_USER: User = {
  id: 'user-2',
  keycloakId: 'kc-2',
  email: 'new@wisdom.dev',
  name: 'New User',
  role: UserRoleEnum.Customer,
};

const MOCK_AUTH_RESPONSE = {
  user: MOCK_USER,
  accessToken: 'token',
  refreshToken: 'refresh',
  expiresIn: 3600,
};

describe('SignUpComponent', () => {
  let fixture: ComponentFixture<SignUpComponent>;
  let component: SignUpComponent;
  let authServiceMock: { signUp: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    authServiceMock = { signUp: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [SignUpComponent],
      providers: [
        provideAnimationsAsync(),
        { provide: AuthService, useValue: authServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SignUpComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have an invalid form when empty', () => {
    const form = (component as any).form;
    expect(form.invalid).toBe(true);
  });

  it('should report passwordMismatch error when passwords differ', () => {
    const form = (component as any).form;
    form.setValue({
      name: 'Test User',
      email: 'test@test.com',
      role: UserRoleEnum.Customer,
      password: 'password123',
      confirmPassword: 'different456',
    });
    form.markAllAsTouched();
    expect(form.hasError('passwordMismatch')).toBe(true);
  });

  it('should not report passwordMismatch when passwords match', () => {
    const form = (component as any).form;
    form.setValue({
      name: 'Test User',
      email: 'test@test.com',
      role: UserRoleEnum.Customer,
      password: 'password123',
      confirmPassword: 'password123',
    });
    expect(form.hasError('passwordMismatch')).toBe(false);
  });

  it('should call authService.signUp without confirmPassword field', fakeAsync(() => {
    authServiceMock.signUp.mockReturnValue(of(MOCK_AUTH_RESPONSE));
    const form = (component as any).form;
    form.setValue({
      name: 'New User',
      email: 'new@wisdom.dev',
      role: UserRoleEnum.Customer,
      password: 'password123',
      confirmPassword: 'password123',
    });

    (component as any).submit();
    tick(1000);

    expect(authServiceMock.signUp).toHaveBeenCalledWith({
      name: 'New User',
      email: 'new@wisdom.dev',
      role: UserRoleEnum.Customer,
      password: 'password123',
    });
  }));

  it('should emit signedUp event on successful registration', fakeAsync(() => {
    authServiceMock.signUp.mockReturnValue(of(MOCK_AUTH_RESPONSE));
    const form = (component as any).form;
    form.setValue({
      name: 'New User',
      email: 'new@wisdom.dev',
      role: UserRoleEnum.Customer,
      password: 'password123',
      confirmPassword: 'password123',
    });

    let emitted: User | undefined;
    component.signedUp.subscribe((u: User) => (emitted = u));

    (component as any).submit();
    tick(1000);

    expect(emitted).toEqual(MOCK_USER);
  }));

  it('should display an error message when email already exists', fakeAsync(() => {
    authServiceMock.signUp.mockReturnValue(
      throwError(() => new Error('An account with this email already exists.')),
    );
    const form = (component as any).form;
    form.setValue({
      name: 'Alice',
      email: 'teacher@wisdom.dev',
      role: UserRoleEnum.Teacher,
      password: 'password123',
      confirmPassword: 'password123',
    });

    (component as any).submit();
    tick(1000);

    fixture.detectChanges();

    const errorEl: HTMLElement = fixture.nativeElement.querySelector('.error-message');
    expect(errorEl?.textContent).toContain('already exists');
  }));
});
