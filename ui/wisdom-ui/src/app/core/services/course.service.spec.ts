import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CourseService } from './course.service';

const TEACHER_ID = '11111111-0000-0000-0000-000000000001';
const STUDENT_ID = '22222222-0000-0000-0000-000000000002';

describe('CourseService', () => {
  let service: CourseService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CourseService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getCoursesByTeacher', () => {
    it('should return courses belonging to the teacher', fakeAsync(() => {
      let courses: any[] = [];
      service.getCoursesByTeacher(TEACHER_ID).subscribe((c) => (courses = c));
      tick(1000);

      expect(courses.length).toBeGreaterThan(0);
      courses.forEach((c) => expect(c.teacherId).toBe(TEACHER_ID));
    }));

    it('should return empty array for unknown teacherId', fakeAsync(() => {
      let courses: any[] = [];
      service.getCoursesByTeacher('unknown-id').subscribe((c) => (courses = c));
      tick(1000);

      expect(courses.length).toBe(0);
    }));
  });

  describe('getEnrolledCourses', () => {
    it('should return courses the student is enrolled in', fakeAsync(() => {
      let courses: any[] = [];
      service.getEnrolledCourses(STUDENT_ID).subscribe((c) => (courses = c));
      tick(1000);

      expect(courses.length).toBeGreaterThan(0);
    }));

    it('should return empty array for user with no enrollments', fakeAsync(() => {
      let courses: any[] = [];
      service.getEnrolledCourses('no-enrollments').subscribe((c) => (courses = c));
      tick(1000);

      expect(courses.length).toBe(0);
    }));
  });

  describe('createCourse', () => {
    it('should create a new course and return it', fakeAsync(() => {
      let created: any;
      service
        .createCourse({ teacherId: TEACHER_ID, title: 'New Course', description: 'A test description', price: 9.99 })
        .subscribe((c) => (created = c));
      tick(1000);

      expect(created).toBeDefined();
      expect(created.title).toBe('New Course');
      expect(created.teacherId).toBe(TEACHER_ID);
      expect(created.isPublished).toBe(false);
      expect(created.id).toBeTruthy();
    }));

    it('should make the new course appear in getCoursesByTeacher', fakeAsync(() => {
      service
        .createCourse({ teacherId: TEACHER_ID, title: 'Extra Course', description: 'Extra description here', price: 5 })
        .subscribe();
      tick(1000);

      let courses: any[] = [];
      service.getCoursesByTeacher(TEACHER_ID).subscribe((c) => (courses = c));
      tick(1000);

      expect(courses.some((c) => c.title === 'Extra Course')).toBe(true);
    }));
  });
});
