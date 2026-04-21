import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { of, throwError } from 'rxjs';
import { SignInComponent } from './sign-in.component';
import { AuthService } from '../../../core/services/auth.service';
import { User, UserRoleEnum } from '../../../core/models/user.model';

const MOCK_USER: User = {
  id: 'user-1',
  keycloakId: 'kc-1',
  email: 'teacher@wisdom.dev',
  name: 'Alice',
  role: UserRoleEnum.Teacher,
};

const MOCK_AUTH_RESPONSE = {
  user: MOCK_USER,
  accessToken: 'token',
  refreshToken: 'refresh',
  expiresIn: 3600,
};

describe('SignInComponent', () => {
  let fixture: ComponentFixture<SignInComponent>;
  let component: SignInComponent;
  let authServiceMock: { signIn: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    authServiceMock = { signIn: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [SignInComponent],
      providers: [
        provideAnimationsAsync(),
        { provide: AuthService, useValue: authServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SignInComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render email and password fields', () => {
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('input[type="email"]')).toBeTruthy();
    expect(el.querySelector('input[type="password"]')).toBeTruthy();
  });

  it('should have an invalid form when empty', () => {
    const form = (component as any).form;
    expect(form.invalid).toBe(true);
  });

  it('should mark all fields as touched when submitting an invalid form', () => {
    const form = (component as any).form;
    (component as any).submit();
    expect(form.get('email')?.touched).toBe(true);
    expect(form.get('password')?.touched).toBe(true);
  });

  it('should call authService.signIn with form values on valid submit', fakeAsync(() => {
    authServiceMock.signIn.mockReturnValue(of(MOCK_AUTH_RESPONSE));
    const form = (component as any).form;
    form.setValue({ email: 'teacher@wisdom.dev', password: 'password123' });

    (component as any).submit();
    tick(1000);

    expect(authServiceMock.signIn).toHaveBeenCalledWith({
      email: 'teacher@wisdom.dev',
      password: 'password123',
    });
  }));

  it('should emit signedIn event on successful sign in', fakeAsync(() => {
    authServiceMock.signIn.mockReturnValue(of(MOCK_AUTH_RESPONSE));
    const form = (component as any).form;
    form.setValue({ email: 'teacher@wisdom.dev', password: 'password123' });

    let emitted: User | undefined;
    component.signedIn.subscribe((u: User) => (emitted = u));

    (component as any).submit();
    tick(1000);

    expect(emitted).toEqual(MOCK_USER);
  }));

  it('should display an error message on failed sign in', fakeAsync(() => {
    authServiceMock.signIn.mockReturnValue(
      throwError(() => new Error('Invalid email or password.')),
    );
    const form = (component as any).form;
    form.setValue({ email: 'wrong@test.com', password: 'password123' });

    (component as any).submit();
    tick(1000);

    fixture.detectChanges();

    const errorEl: HTMLElement = fixture.nativeElement.querySelector('.error-message');
    expect(errorEl?.textContent).toContain('Invalid email or password.');
  }));

  it('should toggle password visibility', () => {
    expect((component as any).hidePassword).toBe(true);
    const toggleBtn: HTMLButtonElement = fixture.nativeElement.querySelector(
      'button[mat-icon-button]',
    );
    toggleBtn.click();
    expect((component as any).hidePassword).toBe(false);
  });
});
