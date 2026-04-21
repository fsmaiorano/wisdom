import { Component, inject } from '@angular/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import {
  EnvironmentOption,
  EnvironmentService,
  ENVIRONMENT_OPTIONS,
} from '../../../core/services/environment.service';

@Component({
  selector: 'app-environment-switcher',
  imports: [MatFormFieldModule, MatSelectModule],
  templateUrl: './environment-switcher.component.html',
  styleUrl: './environment-switcher.component.scss',
})
export class EnvironmentSwitcherComponent {
  private readonly _envService = inject(EnvironmentService);

  readonly options = ENVIRONMENT_OPTIONS;
  readonly activeEnv = this._envService.activeEnv;

  onSelect(option: EnvironmentOption): void {
    this._envService.setEnvironment(option);
  }
}
