import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../../core/services/auth.service';
import { CourseService } from '../../../core/services/course.service';

@Component({
  selector: 'app-create-course',
  imports: [
    ReactiveFormsModule,
    MatToolbarModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: './create-course.component.html',
  styleUrl: './create-course.component.scss',
})
export class CreateCourseComponent {
  protected loading = false;

  private readonly _fb = inject(FormBuilder);
  private readonly _authService = inject(AuthService);
  private readonly _courseService = inject(CourseService);
  private readonly _router = inject(Router);
  private readonly _snackBar = inject(MatSnackBar);

  protected readonly form = this._fb.nonNullable.group({
    title: ['', [Validators.required, Validators.minLength(3)]],
    description: ['', [Validators.required, Validators.minLength(10)]],
    price: [0, [Validators.required, Validators.min(0)]],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const teacher = this._authService.currentUser();
    if (!teacher) return;

    this.loading = true;

    this._courseService
      .createCourse({ teacherId: teacher.id, ...this.form.getRawValue() })
      .subscribe({
        next: (course) => {
          this.loading = false;
          this._snackBar.open(`"${course.title}" created successfully!`, 'Close', { duration: 3000 });
          this._router.navigate(['/dashboard']);
        },
        error: () => {
          this.loading = false;
          this._snackBar.open('Failed to create course. Please try again.', 'Close', { duration: 4000 });
        },
      });
  }

  protected goBack(): void {
    this._router.navigate(['/dashboard']);
  }
}
