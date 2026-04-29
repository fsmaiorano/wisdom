import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { signal } from '@angular/core';
import { AuthComponent } from './auth.component';
import { AuthService } from '../../core/services/auth.service';
import { User, UserRoleEnum } from '../../core/models/user.model';
import { routes } from '../../app.routes';

describe('AuthComponent', () => {
  let fixture: ComponentFixture<AuthComponent>;
  let component: AuthComponent;

  function createAuthServiceStub(authenticated: boolean) {
    return {
      isAuthenticated: () => authenticated,
      currentUser: signal<User | null>(null),
    } as unknown as AuthService;
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AuthComponent],
      providers: [
        provideAnimationsAsync(),
        provideRouter(routes),
        { provide: AuthService, useValue: createAuthServiceStub(false) },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AuthComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render Sign In and Create Account tabs', () => {
    const tabs: NodeListOf<HTMLElement> =
      fixture.nativeElement.querySelectorAll('.mdc-tab__text-label');
    const labels = Array.from(tabs).map((t) => t.textContent?.trim());
    expect(labels).toContain('Sign In');
    expect(labels).toContain('Create Account');
  });

  it('should navigate to /dashboard when already authenticated', async () => {
    await TestBed.resetTestingModule();
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    await TestBed.configureTestingModule({
      imports: [AuthComponent],
      providers: [
        provideAnimationsAsync(),
        provideRouter(routes),
        { provide: AuthService, useValue: createAuthServiceStub(true) },
      ],
    }).compileComponents();

    const f = TestBed.createComponent(AuthComponent);
    f.detectChanges();

    expect(navigateSpy).toHaveBeenCalledWith(['/dashboard']);
  });

  it('should navigate to /dashboard on successful authentication', () => {
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    const mockUser: User = {
      id: '1',
      keycloakId: 'kc',
      email: 'a@b.com',
      name: 'A',
      role: UserRoleEnum.Teacher,
    };

    (component as any).onAuthenticated(mockUser);

    expect(navigateSpy).toHaveBeenCalledWith(['/dashboard']);
  });
});
