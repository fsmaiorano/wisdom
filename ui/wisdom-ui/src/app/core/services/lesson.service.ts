import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { CreateLessonRequest, Lesson } from '../models/lesson.model';
import { CreateLessonApiRequest, LessonApiResponse } from '../models/api.model';
import { EnvironmentService } from './environment.service';

@Injectable({ providedIn: 'root' })
export class LessonService {
  private readonly _http = inject(HttpClient);
  private readonly _env = inject(EnvironmentService);

  getLessonsByCourse(courseId: string): Observable<Lesson[]> {
    return this._http
      .get<LessonApiResponse[]>(`${this._env.apiUrl}/api/courses/${courseId}/lessons`)
      .pipe(map((lessons) => lessons.map(this._mapLesson)));
  }

  getLessonById(courseId: string, lessonId: string): Observable<LessonApiResponse> {
    return this._http.get<LessonApiResponse>(
      `${this._env.apiUrl}/api/courses/${courseId}/lessons/${lessonId}`,
    );
  }

  /** Creates the lesson metadata record. Returns the API response with the real lessonId. */
  createLesson(request: CreateLessonRequest, position = 1): Observable<LessonApiResponse> {
    const body: CreateLessonApiRequest = {
      title: request.title,
      description: request.description,
      position,
      isPreview: false,
    };
    return this._http.post<LessonApiResponse>(
      `${this._env.apiUrl}/api/courses/${request.courseId}/lessons`,
      body,
    );
  }

  updateLesson(
    courseId: string,
    lessonId: string,
    title: string,
    description: string,
    position: number,
    isPreview: boolean,
  ): Observable<LessonApiResponse> {
    return this._http.put<LessonApiResponse>(
      `${this._env.apiUrl}/api/courses/${courseId}/lessons/${lessonId}`,
      { title, description, position, isPreview },
    );
  }

  deleteLesson(courseId: string, lessonId: string): Observable<void> {
    return this._http.delete<void>(
      `${this._env.apiUrl}/api/courses/${courseId}/lessons/${lessonId}`,
    );
  }

  private _mapLesson(l: LessonApiResponse): Lesson {
    return {
      id: l.lessonId,
      courseId: l.courseId,
      order: l.position,
      title: l.title,
      description: l.description,
      contentType: 'video',
      mediaId: l.mediaId,
      resources: [],
    };
  }
}
