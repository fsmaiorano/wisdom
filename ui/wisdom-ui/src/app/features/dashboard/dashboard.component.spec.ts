import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { signal } from '@angular/core';
import { of } from 'rxjs';
import { DashboardComponent } from './dashboard.component';
import { AuthService } from '../../core/services/auth.service';
import { CourseService } from '../../core/services/course.service';
import { User, UserRoleEnum } from '../../core/models/user.model';
import { Course } from '../../core/models/course.model';
import { routes } from '../../app.routes';

const TEACHER: User = {
  id: '11111111-0000-0000-0000-000000000001',
  keycloakId: 'kc-t',
  email: 'teacher@wisdom.dev',
  name: 'Alice Teacher',
  role: UserRoleEnum.Teacher,
};

const STUDENT: User = {
  id: '22222222-0000-0000-0000-000000000002',
  keycloakId: 'kc-s',
  email: 'student@wisdom.dev',
  name: 'Bob Student',
  role: UserRoleEnum.Customer,
};

const MOCK_COURSES: Course[] = [
  {
    id: 'c1',
    teacherId: TEACHER.id,
    title: 'Angular Course',
    description: 'Learn Angular from scratch.',
    price: 49.99,
    isPublished: true,
  },
];

function buildAuthService(user: User) {
  return {
    currentUser: signal<User | null>(user),
    isTeacher: () => user.role === UserRoleEnum.Teacher,
    isStudent: () => user.role === UserRoleEnum.Customer,
    logout: vi.fn(),
  } as unknown as AuthService;
}

describe('DashboardComponent (Teacher)', () => {
  let fixture: ComponentFixture<DashboardComponent>;
  let courseServiceMock: { getCoursesByTeacher: ReturnType<typeof vi.fn>; getEnrolledCourses: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    courseServiceMock = {
      getCoursesByTeacher: vi.fn().mockReturnValue(of(MOCK_COURSES)),
      getEnrolledCourses: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [
        provideAnimationsAsync(),
        provideRouter(routes),
        { provide: AuthService, useValue: buildAuthService(TEACHER) },
        { provide: CourseService, useValue: courseServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should display welcome message with teacher name', fakeAsync(() => {
    tick(500);
    fixture.detectChanges();
    const h1: HTMLElement = fixture.nativeElement.querySelector('.welcome-title');
    expect(h1?.textContent).toContain('Alice Teacher');
  }));

  it('should load and display teacher courses', fakeAsync(() => {
    tick(500);
    fixture.detectChanges();
    const cards: NodeListOf<HTMLElement> = fixture.nativeElement.querySelectorAll('mat-card.course-card');
    expect(cards.length).toBe(1);
    expect(cards[0].textContent).toContain('Angular Course');
  }));

  it('should show New Course button for teachers', fakeAsync(() => {
    tick(500);
    fixture.detectChanges();
    const btn: HTMLElement = fixture.nativeElement.querySelector(
      'button[routerLink="/dashboard/create-course"]',
    );
    expect(btn).toBeTruthy();
  }));

  it('should call getCoursesByTeacher with the teacher id', () => {
    expect(courseServiceMock.getCoursesByTeacher).toHaveBeenCalledWith(TEACHER.id);
  });

  it('should call logout and navigate on logout button click', fakeAsync(() => {
    tick(500);
    fixture.detectChanges();
    const router = TestBed.inject(Router);
    const navSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    const authService = TestBed.inject(AuthService);

    const logoutBtn: HTMLButtonElement = fixture.nativeElement.querySelector(
      'button[mattooltip="Logout"]',
    );
    logoutBtn?.click();

    expect(authService.logout).toHaveBeenCalled();
    expect(navSpy).toHaveBeenCalledWith(['/auth']);
  }));
});

describe('DashboardComponent (Student)', () => {
  let fixture: ComponentFixture<DashboardComponent>;
  let courseServiceMock: { getCoursesByTeacher: ReturnType<typeof vi.fn>; getEnrolledCourses: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    courseServiceMock = {
      getCoursesByTeacher: vi.fn(),
      getEnrolledCourses: vi.fn().mockReturnValue(of(MOCK_COURSES)),
    };

    await TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [
        provideAnimationsAsync(),
        provideRouter(routes),
        { provide: AuthService, useValue: buildAuthService(STUDENT) },
        { provide: CourseService, useValue: courseServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();
  });

  it('should display enrolled courses for students', fakeAsync(() => {
    tick(500);
    fixture.detectChanges();
    const cards: NodeListOf<HTMLElement> = fixture.nativeElement.querySelectorAll('mat-card.course-card');
    expect(cards.length).toBe(1);
  }));

  it('should NOT show New Course button for students', fakeAsync(() => {
    tick(500);
    fixture.detectChanges();
    const btn = fixture.nativeElement.querySelector(
      'button[routerLink="/dashboard/create-course"]',
    );
    expect(btn).toBeFalsy();
  }));

  it('should call getEnrolledCourses with the student id', () => {
    expect(courseServiceMock.getEnrolledCourses).toHaveBeenCalledWith(STUDENT.id);
  });
});
