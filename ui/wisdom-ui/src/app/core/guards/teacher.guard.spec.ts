import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { signal } from '@angular/core';
import { teacherGuard } from './teacher.guard';
import { AuthService } from '../services/auth.service';

describe('teacherGuard', () => {
  function runGuard(isTeacher: boolean) {
    const authServiceStub = {
      isTeacher: signal(isTeacher),
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
      teacherGuard({} as any, {} as any),
    );
  }

  afterEach(() => TestBed.resetTestingModule());

  it('should return true when user is a teacher', () => {
    const result = runGuard(true);
    expect(result).toBe(true);
  });

  it('should return a UrlTree to /dashboard when user is not a teacher', () => {
    const result = runGuard(false);
    expect(result instanceof UrlTree).toBe(true);
  });
});
