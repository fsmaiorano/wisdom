import { Injectable } from '@angular/core';
import { Observable, of, delay } from 'rxjs';
import { Course, CreateCourseRequest } from '../models/course.model';

const MOCK_COURSES: Course[] = [
  {
    id: 'course-0001',
    teacherId: '11111111-0000-0000-0000-000000000001',
    title: 'Introduction to Angular',
    description: 'Learn the fundamentals of Angular from scratch, including components, services, and routing.',
    price: 49.99,
    isPublished: true,
  },
  {
    id: 'course-0002',
    teacherId: '11111111-0000-0000-0000-000000000001',
    title: 'Advanced RxJS Patterns',
    description: 'Deep dive into reactive programming with RxJS, covering operators, subjects, and state management.',
    price: 79.99,
    isPublished: true,
  },
];

const MOCK_ENROLLMENTS: Array<{ userId: string; courseId: string }> = [
  { userId: '22222222-0000-0000-0000-000000000002', courseId: 'course-0001' },
];

@Injectable({ providedIn: 'root' })
export class CourseService {
  getCourseById(courseId: string): Observable<Course | undefined> {
    return of(MOCK_COURSES.find((c) => c.id === courseId)).pipe(delay(300));
  }

  getCoursesByTeacher(teacherId: string): Observable<Course[]> {
    const courses = MOCK_COURSES.filter((c) => c.teacherId === teacherId);
    return of(courses).pipe(delay(400));
  }

  getEnrolledCourses(userId: string): Observable<Course[]> {
    const enrolledIds = MOCK_ENROLLMENTS.filter((e) => e.userId === userId).map((e) => e.courseId);
    const courses = MOCK_COURSES.filter((c) => enrolledIds.includes(c.id));
    return of(courses).pipe(delay(400));
  }

  createCourse(request: CreateCourseRequest): Observable<Course> {
    const newCourse: Course = {
      id: `course-${crypto.randomUUID()}`,
      teacherId: request.teacherId,
      title: request.title,
      description: request.description,
      price: request.price,
      isPublished: false,
    };
    MOCK_COURSES.push(newCourse);
    return of(newCourse).pipe(delay(600));
  }
}
