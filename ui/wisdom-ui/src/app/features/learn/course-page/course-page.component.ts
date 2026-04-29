import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, of } from 'rxjs';
import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CourseService } from '../../../core/services/course.service';
import { OrderService } from '../../../core/services/order.service';
import { CoursePageResponse, LessonWithProgress } from '../../../core/models/api.model';

@Component({
  selector: 'app-course-page',
  imports: [
    CurrencyPipe,
    DecimalPipe,
    MatToolbarModule,
    MatButtonModule,
    MatCardModule,
    MatDividerModule,
    MatIconModule,
    MatListModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTooltipModule,
  ],
  templateUrl: './course-page.component.html',
  styleUrl: './course-page.component.scss',
})
export class CoursePageComponent implements OnInit {
  protected page = signal<CoursePageResponse | null>(null);
  protected loading = signal(true);
  protected enrolling = signal(false);

  private readonly _courseService = inject(CourseService);
  private readonly _orderService = inject(OrderService);
  private readonly _route = inject(ActivatedRoute);
  private readonly _router = inject(Router);
  private readonly _snackBar = inject(MatSnackBar);

  private readonly _courseId = this._route.snapshot.paramMap.get('courseId')!;

  ngOnInit(): void {
    this._courseService
      .getCoursePage(this._courseId)
      .pipe(catchError(() => of(null)))
      .subscribe((page) => {
        if (!page) this._router.navigate(['/learn/catalog']);
        this.page.set(page);
        this.loading.set(false);
      });
  }

  protected get firstLesson(): LessonWithProgress | null {
    const lessons = this.page()?.lessons ?? [];
    return lessons[0] ?? null;
  }

  protected canAccess(lesson: LessonWithProgress): boolean {
    return this.page()?.enrollment.isEnrolled === true || lesson.isPreview;
  }

  protected lessonIcon(lesson: LessonWithProgress): string {
    if (!this.canAccess(lesson)) return 'lock';
    if (lesson.progress?.isCompleted) return 'check_circle';
    return lesson.mediaId ? 'play_circle' : 'article';
  }

  protected watchLesson(lesson: LessonWithProgress): void {
    if (!this.canAccess(lesson)) return;
    this._router.navigate(['/learn', 'courses', this._courseId, 'watch', lesson.lessonId]);
  }

  protected enroll(): void {
    const page = this.page();
    if (!page) return;

    this.enrolling.set(true);
    this._orderService
      .purchaseCourse(this._courseId, page.price)
      .subscribe({
        next: () => {
          this.enrolling.set(false);
          this._snackBar.open('Enrolled successfully!', 'Close', { duration: 3000 });
          // Reload page data so enrollment info updates
          this._courseService.getCoursePage(this._courseId).subscribe((updated) => {
            if (updated) this.page.set(updated);
          });
        },
        error: () => {
          this.enrolling.set(false);
          this._snackBar.open('Enrollment failed. Please try again.', 'Close', { duration: 4000 });
        },
      });
  }

  protected goBack(): void {
    this._router.navigate(['/learn/catalog']);
  }
}
