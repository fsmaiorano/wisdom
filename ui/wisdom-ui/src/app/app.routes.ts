import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { teacherGuard } from './core/guards/teacher.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'auth', pathMatch: 'full' },
  {
    path: 'auth',
    loadComponent: () => import('./features/auth/auth.component').then((m) => m.AuthComponent),
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent),
      },
      {
        path: 'create-course',
        canActivate: [teacherGuard],
        loadComponent: () =>
          import('./features/dashboard/create-course/create-course.component').then(
            (m) => m.CreateCourseComponent,
          ),
      },
      {
        path: 'courses/:courseId',
        canActivate: [teacherGuard],
        loadComponent: () =>
          import('./features/dashboard/course-editor/course-editor.component').then(
            (m) => m.CourseEditorComponent,
          ),
      },
      {
        path: 'courses/:courseId/lessons/new',
        canActivate: [teacherGuard],
        loadComponent: () =>
          import('./features/dashboard/course-editor/create-lesson/create-lesson.component').then(
            (m) => m.CreateLessonComponent,
          ),
      },
      {
        path: 'courses/:courseId/lessons/:lessonId/edit',
        canActivate: [teacherGuard],
        loadComponent: () =>
          import('./features/dashboard/course-editor/lesson-editor/lesson-editor.component').then(
            (m) => m.LessonEditorComponent,
          ),
      },
    ],
  },
  { path: '**', redirectTo: 'auth' },
];
