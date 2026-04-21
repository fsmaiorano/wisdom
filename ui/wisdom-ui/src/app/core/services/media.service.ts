import { inject, Injectable } from '@angular/core';
import {
  HttpClient,
  HttpEvent,
  HttpParams,
  HttpRequest,
  HttpResponse,
} from '@angular/common/http';
import { filter, map, Observable } from 'rxjs';
import { EnvironmentService } from './environment.service';
import { MediaApiResponse, MediaTypeEnum } from '../models/api.model';
import { LessonContentType } from '../models/lesson.model';

@Injectable({ providedIn: 'root' })
export class MediaService {
  private readonly _http = inject(HttpClient);
  private readonly _env = inject(EnvironmentService);

  /** Maps Angular lesson content type to backend MediaTypeEnum. */
  contentTypeToMediaType(contentType: LessonContentType): MediaTypeEnum {
    return contentType === 'video' ? 'Video' : 'Document';
  }

  /**
   * Upload a file to MinIO via the Media API.
   * Emits HttpEvents — filter on HttpEventType.UploadProgress for progress,
   * HttpEventType.Response for the final MediaApiResponse.
   */
  upload(
    file: File,
    lessonId: string,
    courseId: string,
    type: MediaTypeEnum,
  ): Observable<HttpEvent<MediaApiResponse>> {
    const formData = new FormData();
    formData.append('file', file);

    const req = new HttpRequest<FormData>(
      'POST',
      `${this._env.apiUrl}/api/media/upload`,
      formData,
      {
        params: new HttpParams()
          .set('lessonId', lessonId)
          .set('courseId', courseId)
          .set('type', type),
        reportProgress: true,
      },
    );

    return this._http.request<MediaApiResponse>(req);
  }

  /** Convenience: upload and return only the final response body. */
  uploadFile(
    file: File,
    lessonId: string,
    courseId: string,
    type: MediaTypeEnum,
  ): Observable<MediaApiResponse> {
    return this.upload(file, lessonId, courseId, type).pipe(
      filter((e): e is HttpResponse<MediaApiResponse> => e instanceof HttpResponse),
      map((e) => e.body!),
    );
  }
}
