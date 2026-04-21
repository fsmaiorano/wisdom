import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatToolbarModule } from '@angular/material/toolbar';
import { LessonContentType, LESSON_CONTENT_TYPE_LABELS } from '../../../../core/models/lesson.model';
import { LessonService } from '../../../../core/services/lesson.service';

@Component({
  selector: 'app-create-lesson',
  imports: [
    ReactiveFormsModule,
    MatToolbarModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatCardModule,
    MatChipsModule,
    MatDividerModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: './create-lesson.component.html',
  styleUrl: './create-lesson.component.scss',
})
export class CreateLessonComponent {
  protected loading = false;
  protected contentFile = signal<File | null>(null);
  protected resources = signal<File[]>([]);
  protected contentDragOver = signal(false);
  protected resourcesDragOver = signal(false);

  protected readonly contentTypeLabels = LESSON_CONTENT_TYPE_LABELS;
  protected readonly contentTypes: LessonContentType[] = ['video', 'slides', 'text'];

  private readonly _fb = inject(FormBuilder);
  private readonly _lessonService = inject(LessonService);
  private readonly _route = inject(ActivatedRoute);
  private readonly _router = inject(Router);
  private readonly _snackBar = inject(MatSnackBar);

  private readonly _courseId = this._route.snapshot.paramMap.get('courseId')!;

  protected readonly form = this._fb.nonNullable.group({
    title: ['', [Validators.required, Validators.minLength(3)]],
    description: [''],
    contentType: ['video' as LessonContentType, Validators.required],
    contentText: [''],
  });

  get contentType(): LessonContentType {
    return this.form.get('contentType')!.value as LessonContentType;
  }

  protected onMainFileChange(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0] ?? null;
    this.contentFile.set(file);
  }

  protected onContentDrop(event: DragEvent): void {
    event.preventDefault();
    this.contentDragOver.set(false);
    const file = event.dataTransfer?.files?.[0] ?? null;
    if (file) this.contentFile.set(file);
  }

  protected onResourcesDrop(event: DragEvent): void {
    event.preventDefault();
    this.resourcesDragOver.set(false);
    const files = Array.from(event.dataTransfer?.files ?? []);
    if (files.length) this.resources.update((existing) => [...existing, ...files]);
  }

  protected onDragOver(event: DragEvent, target: 'content' | 'resources'): void {
    event.preventDefault();
    if (target === 'content') this.contentDragOver.set(true);
    else this.resourcesDragOver.set(true);
  }

  protected onDragLeave(target: 'content' | 'resources'): void {
    if (target === 'content') this.contentDragOver.set(false);
    else this.resourcesDragOver.set(false);
  }

  protected onResourcesChange(event: Event): void {
    const files = Array.from((event.target as HTMLInputElement).files ?? []);
    this.resources.update((existing) => [...existing, ...files]);
  }

  protected removeResource(index: number): void {
    this.resources.update((list) => list.filter((_, i) => i !== index));
  }

  protected formatBytes(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1048576) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / 1048576).toFixed(1)} MB`;
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { title, description, contentType, contentText } = this.form.getRawValue();

    if ((contentType === 'video' || contentType === 'slides') && !this.contentFile()) {
      this._snackBar.open('Please upload the main content file.', 'Close', { duration: 3000 });
      return;
    }

    this.loading = true;

    this._lessonService
      .createLesson({
        courseId: this._courseId,
        title,
        description,
        contentType,
        contentFile: this.contentFile() ?? undefined,
        contentText: contentType === 'text' ? contentText : undefined,
        resources: this.resources(),
      })
      .subscribe({
        next: (lesson) => {
          this.loading = false;
          this._snackBar.open(`Lesson "${lesson.title}" created!`, 'Close', { duration: 3000 });
          this._router.navigate(['/dashboard/courses', this._courseId]);
        },
        error: () => {
          this.loading = false;
          this._snackBar.open('Failed to create lesson. Please try again.', 'Close', { duration: 4000 });
        },
      });
  }

  protected goBack(): void {
    this._router.navigate(['/dashboard/courses', this._courseId]);
  }
}
