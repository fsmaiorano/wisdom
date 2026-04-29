import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import {
  CourseApiResponse,
  CoursePageResponse,
  CreateCourseApiRequest,
  DashboardResponse,
  EnrolledCourseItem,
} from '../models/api.model';
import { Course, CreateCourseRequest } from '../models/course.model';
import { EnvironmentService } from './environment.service';

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

  getAllPublished(): Observable<Course[]> {
    return this._http
      .get<CourseApiResponse[]>(this._baseUrl)
      .pipe(map((courses) => courses.map(mapApiCourse)));
  }

  getCourseById(courseId: string): Observable<Course> {
    return this._http
      .get<CourseApiResponse>(`${this._baseUrl}/${courseId}`)
      .pipe(map(mapApiCourse));
  }

  getCoursesByTeacher(_teacherId: string): Observable<Course[]> {
    return this._http
      .get<CourseApiResponse[]>(`${this._baseUrl}/my`)
      .pipe(map((courses) => courses.map(mapApiCourse)));
  }

  getEnrolledCourses(): Observable<EnrolledCourseItem[]> {
    return this._http
      .get<DashboardResponse>(`${this._env.apiUrl}/bff/dashboard`)
      .pipe(map((r) => r.enrolledCourses));
  }

  getCoursePage(courseId: string): Observable<CoursePageResponse> {
    return this._http.get<CoursePageResponse>(
      `${this._env.apiUrl}/bff/courses/${courseId}/page`,
    );
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
