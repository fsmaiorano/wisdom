import {
  Component,
  computed,
  ElementRef,
  inject,
  OnDestroy,
  OnInit,
  signal,
  ViewChild,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, of } from 'rxjs';
import { DecimalPipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CourseService } from '../../../core/services/course.service';
import { MediaService } from '../../../core/services/media.service';
import { ProgressService } from '../../../core/services/progress.service';
import { EnvironmentService } from '../../../core/services/environment.service';
import { CoursePageResponse, LessonWithProgress, MediaApiResponse } from '../../../core/models/api.model';

type Quality = '480p' | '720p' | '1080p';

@Component({
  selector: 'app-watch',
  imports: [
    DecimalPipe,
    MatToolbarModule,
    MatButtonModule,
    MatDividerModule,
    MatIconModule,
    MatListModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSnackBarModule,
    MatTooltipModule,
  ],
  templateUrl: './watch.component.html',
  styleUrl: './watch.component.scss',
})
export class WatchComponent implements OnInit, OnDestroy {
  @ViewChild('videoEl') videoEl?: ElementRef<HTMLVideoElement>;

  protected page = signal<CoursePageResponse | null>(null);
  protected currentLesson = signal<LessonWithProgress | null>(null);
  protected lessonMedia = signal<MediaApiResponse[]>([]);
  protected loading = signal(true);
  protected quality = signal<Quality>('720p');
  protected watchedSeconds = signal(0);
  protected readonly qualities: Quality[] = ['480p', '720p', '1080p'];

  protected readonly mainVideo = computed(() =>
    this.lessonMedia().find((m) => m.type === 'Video') ?? null,
  );
  protected readonly resourceFiles = computed(() =>
    this.lessonMedia().filter((m) => m.type !== 'Video'),
  );

  private readonly _courseService = inject(CourseService);
  private readonly _mediaService = inject(MediaService);
  private readonly _progressService = inject(ProgressService);
  private readonly _env = inject(EnvironmentService);
  private readonly _route = inject(ActivatedRoute);
  private readonly _router = inject(Router);
  private readonly _snackBar = inject(MatSnackBar);

  private readonly _courseId = this._route.snapshot.paramMap.get('courseId')!;
  private readonly _lessonId = this._route.snapshot.paramMap.get('lessonId')!;

  private _progressInterval?: ReturnType<typeof setInterval>;
  private _lastSavedSeconds = 0;

  ngOnInit(): void {
    this._courseService
      .getCoursePage(this._courseId)
      .pipe(catchError(() => of(null)))
      .subscribe((page) => {
        if (!page) {
          this._router.navigate(['/learn/catalog']);
          return;
        }
        this.page.set(page);

        const lesson = page.lessons.find((l) => l.lessonId === this._lessonId)
          ?? page.lessons[0]
          ?? null;

        if (!page.enrollment.isEnrolled && lesson && !lesson.isPreview) {
          this._router.navigate(['/learn', 'courses', this._courseId]);
          return;
        }

        this.loading.set(false);
        this._selectLesson(lesson, page);
      });
  }

  ngOnDestroy(): void {
    this._clearProgressInterval();
    this._saveProgress(false);
  }

  protected streamUrl(mediaId: string): string {
    return `${this._env.apiUrl}/stream/${mediaId}?quality=${this.quality()}`;
  }

  protected get currentLessonIndex(): number {
    const lessons = this.page()?.lessons ?? [];
    return lessons.findIndex((l) => l.lessonId === this.currentLesson()?.lessonId);
  }

  protected get prevLesson(): LessonWithProgress | null {
    const lessons = this.page()?.lessons ?? [];
    const idx = this.currentLessonIndex;
    return idx > 0 ? lessons[idx - 1] : null;
  }

  protected get nextLesson(): LessonWithProgress | null {
    const lessons = this.page()?.lessons ?? [];
    const idx = this.currentLessonIndex;
    return idx >= 0 && idx < lessons.length - 1 ? lessons[idx + 1] : null;
  }

  protected canAccess(lesson: LessonWithProgress): boolean {
    return this.page()?.enrollment.isEnrolled === true || lesson.isPreview;
  }

  protected selectLesson(lesson: LessonWithProgress): void {
    if (!this.canAccess(lesson)) return;
    this._saveProgress(false);
    this._clearProgressInterval();
    const page = this.page();
    if (page) this._selectLesson(lesson, page);
    this._router.navigate(
      ['/learn', 'courses', this._courseId, 'watch', lesson.lessonId],
      { replaceUrl: true },
    );
  }

  protected onTimeUpdate(event: Event): void {
    const video = event.target as HTMLVideoElement;
    this.watchedSeconds.set(Math.floor(video.currentTime));
  }

  protected onVideoEnded(): void {
    this._clearProgressInterval();
    this._saveProgress(true);
    const lesson = this.currentLesson();
    if (lesson) {
      this.page.update((p) => {
        if (!p) return p;
        return {
          ...p,
          lessons: p.lessons.map((l) =>
            l.lessonId === lesson.lessonId
              ? { ...l, progress: { watchedSeconds: this.watchedSeconds(), isCompleted: true } }
              : l,
          ),
          progress: {
            ...p.progress,
            completedLessons: p.progress.completedLessons + (lesson.progress?.isCompleted ? 0 : 1),
            percentage: Math.round(
              ((p.progress.completedLessons + (lesson.progress?.isCompleted ? 0 : 1)) /
                p.progress.totalLessons) *
                100,
            ),
          },
        };
      });
    }
  }

  protected onQualityChange(q: Quality): void {
    const video = this.videoEl?.nativeElement;
    const currentTime = video?.currentTime ?? 0;
    this.quality.set(q);
    // After Angular updates the src binding, restore playback position
    setTimeout(() => {
      if (video) {
        video.currentTime = currentTime;
        video.play().catch(() => {});
      }
    }, 200);
  }

  protected resourceIcon(filePath: string): string {
    const ext = filePath.split('.').pop()?.toLowerCase() ?? '';
    if (['pdf'].includes(ext)) return 'picture_as_pdf';
    if (['xls', 'xlsx', 'csv'].includes(ext)) return 'table_chart';
    if (['zip', 'rar', 'tar', 'gz'].includes(ext)) return 'archive';
    return 'insert_drive_file';
  }

  protected resourceName(filePath: string): string {
    return filePath.split('/').pop() ?? filePath;
  }

  protected lessonStateIcon(lesson: LessonWithProgress): string {
    if (lesson.lessonId === this.currentLesson()?.lessonId) return 'play_arrow';
    if (lesson.progress?.isCompleted) return 'check_circle';
    if (!this.canAccess(lesson)) return 'lock';
    return 'radio_button_unchecked';
  }

  protected goBack(): void {
    this._router.navigate(['/learn', 'courses', this._courseId]);
  }

  protected onMetadataLoaded(): void {
    const video = this.videoEl?.nativeElement;
    if (video) {
      video.currentTime = this.watchedSeconds();
    }
  }

  private _selectLesson(lesson: LessonWithProgress | null, page: CoursePageResponse): void {
    this.currentLesson.set(lesson);
    this.watchedSeconds.set(lesson?.progress?.watchedSeconds ?? 0);
    this._lastSavedSeconds = lesson?.progress?.watchedSeconds ?? 0;
    this.lessonMedia.set([]);

    if (lesson) {
      this._mediaService
        .getMediaByLesson(lesson.lessonId)
        .pipe(catchError(() => of([])))
        .subscribe((media) => this.lessonMedia.set(media));
    }

    if (page.enrollment.isEnrolled && lesson) {
      this._startProgressInterval(lesson.lessonId);
    }
  }

  private _startProgressInterval(lessonId: string): void {
    this._progressInterval = setInterval(() => {
      const secs = this.watchedSeconds();
      if (secs > this._lastSavedSeconds) {
        this._progressService
          .updateProgress(lessonId, secs, false)
          .pipe(catchError(() => of(null)))
          .subscribe(() => { this._lastSavedSeconds = secs; });
      }
    }, 10_000);
  }

  private _clearProgressInterval(): void {
    if (this._progressInterval) {
      clearInterval(this._progressInterval);
      this._progressInterval = undefined;
    }
  }

  private _saveProgress(isCompleted: boolean): void {
    const lesson = this.currentLesson();
    const secs = this.watchedSeconds();
    if (!lesson || secs <= this._lastSavedSeconds) return;

    this._progressService
      .updateProgress(lesson.lessonId, secs, isCompleted)
      .pipe(catchError(() => of(null)))
      .subscribe(() => { this._lastSavedSeconds = secs; });
  }
}
