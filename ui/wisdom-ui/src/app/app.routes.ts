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
    ],
  },
  { path: '**', redirectTo: 'auth' },
];
