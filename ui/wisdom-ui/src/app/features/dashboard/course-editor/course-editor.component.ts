import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { catchError, of, switchMap } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CurrencyPipe } from '@angular/common';
import { CourseService } from '../../../core/services/course.service';
import { LessonService } from '../../../core/services/lesson.service';
import { Course } from '../../../core/models/course.model';
import { Lesson, LESSON_CONTENT_TYPE_LABELS } from '../../../core/models/lesson.model';

@Component({
  selector: 'app-course-editor',
  imports: [
    RouterLink,
    CurrencyPipe,
    MatToolbarModule,
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatDividerModule,
    MatIconModule,
    MatListModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
  ],
  templateUrl: './course-editor.component.html',
  styleUrl: './course-editor.component.scss',
})
export class CourseEditorComponent implements OnInit {
  protected course = signal<Course | null>(null);
  protected lessons = signal<Lesson[]>([]);
  protected loading = signal(true);

  protected readonly contentTypeLabels = LESSON_CONTENT_TYPE_LABELS;

  private readonly _courseService = inject(CourseService);
  private readonly _lessonService = inject(LessonService);
  private readonly _route = inject(ActivatedRoute);
  private readonly _router = inject(Router);

  private readonly _courseId = this._route.snapshot.paramMap.get('courseId')!;

  ngOnInit(): void {
    this._courseService
      .getCourseById(this._courseId)
      .pipe(
        catchError(() => of(undefined)),
        switchMap((course) => {
          this.course.set(course ?? null);
          if (!course) {
            this._router.navigate(['/dashboard']);
            return of([]);
          }
          return this._lessonService
            .getLessonsByCourse(this._courseId)
            .pipe(catchError(() => of([])));
        }),
      )
      .subscribe((lessons) => {
        this.lessons.set(lessons as Lesson[]);
        this.loading.set(false);
      });
  }

  protected contentTypeIcon(lesson: Lesson): string {
    return { video: 'videocam', document: 'insert_drive_file' }[lesson.contentType];
  }

  protected goBack(): void {
    this._router.navigate(['/dashboard']);
  }
}
