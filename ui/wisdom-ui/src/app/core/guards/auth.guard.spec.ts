import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { signal } from '@angular/core';
import { authGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';

describe('authGuard', () => {
  function runGuard(isAuthenticated: boolean) {
    const authServiceStub = {
      isAuthenticated: signal(isAuthenticated),
    };

    const urlTree = new UrlTree();
    const routerStub = {
      createUrlTree: vi.fn().mockReturnValue(urlTree),
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authServiceStub },
        { provide: Router, useValue: routerStub },
      ],
    });

    return TestBed.runInInjectionContext(() =>
      authGuard({} as any, {} as any),
    );
  }

  afterEach(() => TestBed.resetTestingModule());

  it('should return true when user is authenticated', () => {
    const result = runGuard(true);
    expect(result).toBe(true);
  });

  it('should return a UrlTree to /auth when user is not authenticated', () => {
    const result = runGuard(false);
    expect(result instanceof UrlTree).toBe(true);
  });
});
