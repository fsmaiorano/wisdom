import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ProgressResponse } from '../models/api.model';
import { EnvironmentService } from './environment.service';

@Injectable({ providedIn: 'root' })
export class ProgressService {
  private readonly _http = inject(HttpClient);
  private readonly _env = inject(EnvironmentService);

  updateProgress(
    lessonId: string,
    watchedSeconds: number,
    isCompleted: boolean,
  ): Observable<ProgressResponse> {
    return this._http.post<ProgressResponse>(`${this._env.apiUrl}/api/progress`, {
      lessonId,
      watchedSeconds,
      isCompleted,
    });
  }
}
