import { Injectable, signal } from '@angular/core';
import { environment } from '../../../environments/environment';

export interface EnvironmentOption {
  name: string;
  apiUrl: string;
}

export const ENVIRONMENT_OPTIONS: EnvironmentOption[] = [
  { name: 'Local', apiUrl: 'http://localhost:5000' },
  { name: 'Docker', apiUrl: 'http://localhost:6100' },
];

const STORAGE_KEY = 'wisdom_env';

@Injectable({ providedIn: 'root' })
export class EnvironmentService {
  readonly isDev = !environment.production;

  private readonly _activeEnv = signal<EnvironmentOption>(this._loadOrDefault());
  readonly activeEnv = this._activeEnv.asReadonly();

  get apiUrl(): string {
    return this._activeEnv().apiUrl;
  }

  setEnvironment(option: EnvironmentOption): void {
    this._activeEnv.set(option);
    if (this.isDev) {
      localStorage.setItem(STORAGE_KEY, option.apiUrl);
    }
  }

  private _loadOrDefault(): EnvironmentOption {
    const buildDefault =
      ENVIRONMENT_OPTIONS.find((e) => e.apiUrl === environment.apiUrl) ?? ENVIRONMENT_OPTIONS[0];

    if (environment.production) {
      return buildDefault;
    }
    try {
      const saved = localStorage.getItem(STORAGE_KEY);
      return ENVIRONMENT_OPTIONS.find((e) => e.apiUrl === saved) ?? buildDefault;
    } catch {
      return buildDefault;
    }
  }
}
