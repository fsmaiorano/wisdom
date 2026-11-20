import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, forkJoin, of, switchMap } from 'rxjs';
import { LessonApiResponse, MediaApiResponse, MediaTypeEnum } from '../../../../core/models/api.model';
import { LessonService } from '../../../../core/services/lesson.service';
import { MediaService } from '../../../../core/services/media.service';

type UploadMediaType = Extract<MediaTypeEnum, 'Video' | 'Document'>;

@Component({
  selector: 'app-lesson-editor',
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
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule,
    MatSnackBarModule,
    MatTooltipModule,
  ],
  templateUrl: './lesson-editor.component.html',
  styleUrl: './lesson-editor.component.scss',
})
export class LessonEditorComponent implements OnInit {
  protected lesson = signal<LessonApiResponse | null>(null);
  protected media = signal<MediaApiResponse[]>([]);
  protected loading = signal(true);
  protected saving = signal(false);
  protected deleting = signal<Set<string>>(new Set());
  protected newResources = signal<File[]>([]);
  protected newResourceType = signal<UploadMediaType>('Video');
  protected resourcesDragOver = signal(false);
  protected uploadProgress = signal({ done: 0, total: 0 });

  private readonly _fb = inject(FormBuilder);
  private readonly _lessonService = inject(LessonService);
  private readonly _mediaService = inject(MediaService);
  private readonly _route = inject(ActivatedRoute);
  private readonly _router = inject(Router);
  private readonly _snackBar = inject(MatSnackBar);

  private readonly _courseId = this._route.snapshot.paramMap.get('courseId')!;
  private readonly _lessonId = this._route.snapshot.paramMap.get('lessonId')!;

  protected readonly form = this._fb.nonNullable.group({
    title: ['', [Validators.required, Validators.minLength(3)]],
    description: [''],
    isPreview: [false],
  });

  ngOnInit(): void {
    this._lessonService
      .getLessonById(this._courseId, this._lessonId)
      .pipe(
        catchError(() => of(null)),
        switchMap((lesson) => {
          if (!lesson) {
            this._router.navigate(['/dashboard/courses', this._courseId]);
            return of([]);
          }
          this.lesson.set(lesson);
          this.form.patchValue({
            title: lesson.title,
            description: lesson.description,
            isPreview: lesson.isPreview,
          });
          return this._mediaService
            .getMediaByLesson(this._lessonId)
            .pipe(catchError(() => of([])));
        }),
      )
      .subscribe((mediaList) => {
        this.media.set(mediaList as MediaApiResponse[]);
        this.loading.set(false);
      });
  }

  protected mediaTypeIcon(type: string): string {
    return type === 'Video' ? 'videocam' : type === 'Image' ? 'image' : 'insert_drive_file';
  }

  protected mediaTypeLabel(type: string): string {
    return type === 'Video' ? 'Video' : type === 'Image' ? 'Image' : 'Document';
  }

  protected isDeleting(mediaId: string): boolean {
    return this.deleting().has(mediaId);
  }

  protected deleteMedia(mediaId: string): void {
    this.deleting.update((s) => new Set([...s, mediaId]));
    this._mediaService.deleteMedia(mediaId).subscribe({
      next: () => {
        this.media.update((list) => list.filter((m) => m.mediaId !== mediaId));
        this.deleting.update((s) => {
          const next = new Set(s);
          next.delete(mediaId);
          return next;
        });
        this._snackBar.open('File removed.', 'Close', { duration: 2000 });
      },
      error: () => {
        this.deleting.update((s) => {
          const next = new Set(s);
          next.delete(mediaId);
          return next;
        });
        this._snackBar.open('Failed to remove file. Try again.', 'Close', { duration: 3000 });
      },
    });
  }

  protected onResourcesDrop(event: DragEvent): void {
    event.preventDefault();
    this.resourcesDragOver.set(false);
    const files = Array.from(event.dataTransfer?.files ?? []);
    if (files.length) this.newResources.update((existing) => [...existing, ...files]);
  }

  protected onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.resourcesDragOver.set(true);
  }

  protected onDragLeave(): void {
    this.resourcesDragOver.set(false);
  }

  protected onResourcesChange(event: Event): void {
    const files = Array.from((event.target as HTMLInputElement).files ?? []);
    this.newResources.update((existing) => [...existing, ...files]);
  }

  protected removePendingResource(index: number): void {
    this.newResources.update((list) => list.filter((_, i) => i !== index));
  }

  protected setNewResourceType(type: UploadMediaType): void {
    this.newResourceType.set(type);
  }

  protected formatBytes(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1048576) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / 1048576).toFixed(1)} MB`;
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const lesson = this.lesson();
    if (!lesson) return;

    const { title, description, isPreview } = this.form.getRawValue();
    this.saving.set(true);

    this._lessonService
      .updateLesson(this._courseId, this._lessonId, title, description, lesson.position, isPreview)
      .pipe(
        switchMap((updated) => {
          this.lesson.set(updated);
          const files = this.newResources();
          if (files.length === 0) return of(updated);

          this.uploadProgress.set({ done: 0, total: files.length });
          const type = this.newResourceType();
          const uploads = files.map((file, i) =>
            this._mediaService
              .uploadFile(file, this._lessonId, this._courseId, type)
              .pipe(
                switchMap((media) => {
                  this.uploadProgress.set({ done: i + 1, total: files.length });
                  this.media.update((list) => [...list, media]);
                  return of(media);
                }),
              ),
          );
          return forkJoin(uploads).pipe(switchMap(() => of(updated)));
        }),
      )
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.newResources.set([]);
          this.uploadProgress.set({ done: 0, total: 0 });
          this._snackBar.open('Lesson saved!', 'Close', { duration: 2500 });
        },
        error: () => {
          this.saving.set(false);
          this._snackBar.open('Failed to save lesson. Please try again.', 'Close', {
            duration: 4000,
          });
        },
      });
  }

  protected goBack(): void {
    this._router.navigate(['/dashboard/courses', this._courseId]);
  }
}
