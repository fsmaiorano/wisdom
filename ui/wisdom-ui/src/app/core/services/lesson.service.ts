import { Injectable } from '@angular/core';
import { delay, Observable, of } from 'rxjs';
import { CreateLessonRequest, Lesson } from '../models/lesson.model';

const MOCK_LESSONS: Lesson[] = [
  {
    id: 'lesson-0001',
    courseId: 'course-0001',
    order: 1,
    title: 'What is Angular?',
    description: 'A brief overview of the Angular framework and its architecture.',
    contentType: 'video',
    contentFileName: 'intro-to-angular.mp4',
    resources: [
      { id: 'res-0001', name: 'Angular Cheatsheet.pdf', mimeType: 'application/pdf', size: 204800 },
    ],
  },
  {
    id: 'lesson-0002',
    courseId: 'course-0001',
    order: 2,
    title: 'Components & Templates',
    description: 'Learn how Angular components and templates work together.',
    contentType: 'slides',
    contentFileName: 'components-templates.pdf',
    resources: [],
  },
  {
    id: 'lesson-0003',
    courseId: 'course-0002',
    order: 1,
    title: 'Introduction to Observables',
    description: 'Understanding the Observable pattern and how RxJS uses it.',
    contentType: 'text',
    contentText:
      'An Observable is a lazy Push collection of multiple values. ' +
      'Unlike Promises, Observables are not executed until subscribed to, ' +
      'and can emit multiple values over time.',
    resources: [
      { id: 'res-0002', name: 'rxjs-operators.xlsx', mimeType: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', size: 51200 },
      { id: 'res-0003', name: 'examples.zip', mimeType: 'application/zip', size: 102400 },
    ],
  },
];

@Injectable({ providedIn: 'root' })
export class LessonService {
  getLessonsByCourse(courseId: string): Observable<Lesson[]> {
    return of(MOCK_LESSONS.filter((l) => l.courseId === courseId)).pipe(delay(400));
  }

  createLesson(request: CreateLessonRequest): Observable<Lesson> {
    const existing = MOCK_LESSONS.filter((l) => l.courseId === request.courseId);
    const lesson: Lesson = {
      id: `lesson-${crypto.randomUUID()}`,
      courseId: request.courseId,
      order: existing.length + 1,
      title: request.title,
      description: request.description,
      contentType: request.contentType,
      contentFileName: request.contentFile?.name,
      contentText: request.contentText,
      resources: request.resources.map((f) => ({
        id: crypto.randomUUID(),
        name: f.name,
        mimeType: f.type,
        size: f.size,
      })),
    };
    MOCK_LESSONS.push(lesson);
    return of(lesson).pipe(delay(600));
  }
}
