import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable, of } from 'rxjs';
import { CourseApiResponse, CreateCourseApiRequest } from '../models/api.model';
import { Course, CreateCourseRequest } from '../models/course.model';
import { EnvironmentService } from './environment.service';

const MOCK_ENROLLMENTS: Array<{ userId: string; courseId: string }> = [];

function mapApiCourse(c: CourseApiResponse): Course {
  return {
    id: c.courseId,
    teacherId: c.teacherId,
    title: c.title,
    description: c.description,
    price: c.price,
    isPublished: c.isPublished,
  };
}

@Injectable({ providedIn: 'root' })
export class CourseService {
  private readonly _http = inject(HttpClient);
  private readonly _env = inject(EnvironmentService);

  private get _baseUrl(): string {
    return `${this._env.apiUrl}/api/courses`;
  }

  getCourseById(courseId: string): Observable<Course | undefined> {
    return this._http
      .get<CourseApiResponse>(`${this._baseUrl}/${courseId}`)
      .pipe(map(mapApiCourse));
  }

  getCoursesByTeacher(_teacherId: string): Observable<Course[]> {
    return this._http
      .get<CourseApiResponse[]>(`${this._baseUrl}/my`)
      .pipe(map((courses) => courses.map(mapApiCourse)));
  }

  getEnrolledCourses(userId: string): Observable<Course[]> {
    // TODO: replace with BFF /bff/dashboard once Keycloak auth is integrated
    const enrolledIds = MOCK_ENROLLMENTS.filter((e) => e.userId === userId).map((e) => e.courseId);
    return of(enrolledIds.map(() => [] as Course[]).flat());
  }

  createCourse(request: CreateCourseRequest): Observable<Course> {
    const body: CreateCourseApiRequest = {
      title: request.title,
      description: request.description,
      price: request.price,
    };
    return this._http
      .post<CourseApiResponse>(this._baseUrl, body)
      .pipe(map(mapApiCourse));
  }
}
