import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { PurchaseCourseResponse } from '../models/api.model';
import { EnvironmentService } from './environment.service';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly _http = inject(HttpClient);
  private readonly _env = inject(EnvironmentService);

  purchaseCourse(courseId: string, price: number): Observable<PurchaseCourseResponse> {
    return this._http.post<PurchaseCourseResponse>(
      `${this._env.apiUrl}/api/orders/purchase`,
      { courseId, price },
    );
  }

  checkEnrollment(courseId: string): Observable<boolean> {
    return this._http
      .get<{ isEnrolled: boolean }>(
        `${this._env.apiUrl}/api/orders/enrollments/${courseId}/check`,
      )
      .pipe(map((r) => r.isEnrolled));
  }
}
