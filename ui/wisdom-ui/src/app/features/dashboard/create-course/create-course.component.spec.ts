import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { signal } from '@angular/core';
import { of, throwError } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CreateCourseComponent } from './create-course.component';
import { AuthService } from '../../../core/services/auth.service';
import { CourseService } from '../../../core/services/course.service';
import { User, UserRoleEnum } from '../../../core/models/user.model';
import { Course } from '../../../core/models/course.model';
import { routes } from '../../../app.routes';

const TEACHER: User = {
  id: 'teacher-1',
  keycloakId: 'kc-t',
  email: 'teacher@wisdom.dev',
  name: 'Alice',
  role: UserRoleEnum.Teacher,
};

const NEW_COURSE: Course = {
  id: 'new-course-1',
  teacherId: TEACHER.id,
  title: 'New Angular Course',
  description: 'A comprehensive Angular course for beginners.',
  price: 29.99,
  isPublished: false,
};

describe('CreateCourseComponent', () => {
  let fixture: ComponentFixture<CreateCourseComponent>;
  let component: CreateCourseComponent;
  let courseServiceMock: { createCourse: ReturnType<typeof vi.fn> };
  let snackBarMock: { open: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    const authServiceStub = { currentUser: signal(TEACHER) };
    courseServiceMock = { createCourse: vi.fn() };
    snackBarMock = { open: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [CreateCourseComponent],
      providers: [
        provideRouter(routes),
        provideAnimationsAsync(),
        { provide: AuthService, useValue: authServiceStub },
        { provide: CourseService, useValue: courseServiceMock },
        { provide: MatSnackBar, useValue: snackBarMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CreateCourseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render title, description, and price fields', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('input[formControlName="title"]')).toBeTruthy();
    expect(compiled.querySelector('textarea[formControlName="description"]')).toBeTruthy();
    expect(compiled.querySelector('input[formControlName="price"]')).toBeTruthy();
  });

  it('should mark form invalid and not call service if form is empty', () => {
    (component as any).submit();
    expect(courseServiceMock.createCourse).not.toHaveBeenCalled();
    expect((component as any).form.touched).toBe(true);
  });

  it('should call createCourse with correct data and navigate to dashboard on success', fakeAsync(() => {
    courseServiceMock.createCourse.mockReturnValue(of(NEW_COURSE));
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    const form = (component as any).form;
    form.setValue({ title: NEW_COURSE.title, description: NEW_COURSE.description, price: NEW_COURSE.price });

    (component as any).submit();
    tick(100);

    expect(courseServiceMock.createCourse).toHaveBeenCalledWith({
      teacherId: TEACHER.id,
      title: NEW_COURSE.title,
      description: NEW_COURSE.description,
      price: NEW_COURSE.price,
    });
    expect(snackBarMock.open).toHaveBeenCalledWith(
      expect.stringContaining('created successfully'),
      'Close',
      expect.any(Object),
    );
    expect(navigateSpy).toHaveBeenCalledWith(['/dashboard']);
  }));

  it('should show error snack bar on createCourse failure', fakeAsync(() => {
    courseServiceMock.createCourse.mockReturnValue(throwError(() => new Error('Server error')));

    const form = (component as any).form;
    form.setValue({ title: 'Valid Title', description: 'Valid description text here', price: 10 });

    (component as any).submit();
    tick();

    expect(snackBarMock.open).toHaveBeenCalledWith(
      expect.stringContaining('Failed to create'),
      'Close',
      expect.any(Object),
    );
  }));

  it('should navigate to /dashboard when goBack is called', () => {
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    (component as any).goBack();
    expect(navigateSpy).toHaveBeenCalledWith(['/dashboard']);
  });
});
