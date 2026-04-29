import { Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { catchError, of } from 'rxjs';
import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../../core/services/auth.service';
import { CourseService } from '../../core/services/course.service';
import { Course } from '../../core/models/course.model';
import { EnrolledCourseItem } from '../../core/models/api.model';
import { User, UserRoleEnum, USER_ROLE_LABELS } from '../../core/models/user.model';

@Component({
  selector: 'app-dashboard',
  imports: [
    RouterLink,
    CurrencyPipe,
    DecimalPipe,
    MatToolbarModule,
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatDividerModule,
    MatIconModule,
    MatListModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  protected readonly authService = inject(AuthService);
  private readonly _courseService = inject(CourseService);
  private readonly _router = inject(Router);

  protected readonly user = signal<User | null>(null);
  protected readonly courses = signal<Course[]>([]);
  protected readonly enrolledCourses = signal<EnrolledCourseItem[]>([]);
  protected readonly loading = signal(true);

  ngOnInit(): void {
    const currentUser = this.authService.currentUser();
    if (!currentUser) return;

    this.user.set(currentUser);

    if (this.authService.isTeacher()) {
      this._courseService
        .getCoursesByTeacher(currentUser.id)
        .pipe(catchError(() => of([])))
        .subscribe((c) => {
          this.courses.set(c);
          this.loading.set(false);
        });
    } else if (this.authService.isStudent()) {
      this._courseService
        .getEnrolledCourses()
        .pipe(catchError(() => of([])))
        .subscribe((c) => {
          this.enrolledCourses.set(c);
          this.loading.set(false);
        });
    } else {
      this.loading.set(false);
    }
  }

  protected continueUrl(courseId: string, lessons: number): string[] {
    return lessons > 0
      ? ['/learn', 'courses', courseId]
      : ['/learn', 'courses', courseId];
  }

  protected logout(): void {
    this.authService.logout();
    this._router.navigate(['/auth']);
  }

  protected roleLabel(user: User): string {
    return USER_ROLE_LABELS[user.role];
  }
}
