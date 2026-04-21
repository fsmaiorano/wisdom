import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTabGroup, MatTabsModule } from '@angular/material/tabs';
import { MatDividerModule } from '@angular/material/divider';
import { MatButtonModule } from '@angular/material/button';
import { SignInComponent } from './sign-in/sign-in.component';
import { SignUpComponent } from './sign-up/sign-up.component';
import { AuthService } from '../../core/services/auth.service';
import { EnvironmentService } from '../../core/services/environment.service';
import { EnvironmentSwitcherComponent } from '../../shared/components/environment-switcher/environment-switcher.component';
import { User } from '../../core/models/user.model';

const DEV_CREDENTIALS = {
  teacher: { email: 'teacher@wisdom.dev', password: 'demoteacher' },
  customer: { email: 'customer@wisdom.dev', password: 'democustomer' },
} as const;

@Component({
  selector: 'app-auth',
  imports: [
    MatCardModule,
    MatTabsModule,
    MatDividerModule,
    MatButtonModule,
    SignInComponent,
    SignUpComponent,
    EnvironmentSwitcherComponent,
  ],
  templateUrl: './auth.component.html',
  styleUrl: './auth.component.scss',
})
export class AuthComponent implements OnInit {
  @ViewChild(MatTabGroup) private readonly _tabs!: MatTabGroup;
  @ViewChild(SignInComponent) private readonly _signIn!: SignInComponent;

  private readonly _authService = inject(AuthService);
  private readonly _router = inject(Router);
  protected readonly envService = inject(EnvironmentService);

  ngOnInit(): void {
    if (this._authService.isAuthenticated()) {
      this._router.navigate(['/dashboard']);
    }
  }

  protected fillCredential(role: keyof typeof DEV_CREDENTIALS): void {
    this._tabs.selectedIndex = 0;
    setTimeout(() => this._signIn.fillCredentials(
      DEV_CREDENTIALS[role].email,
      DEV_CREDENTIALS[role].password,
    ));
  }

  protected onAuthenticated(_user: User): void {
    this._router.navigate(['/dashboard']);
  }
}
