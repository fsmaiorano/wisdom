import { Component, inject, output, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../../core/services/auth.service';
import { User, UserRoleEnum } from '../../../core/models/user.model';

function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password')?.value;
  const confirm = control.get('confirmPassword')?.value;
  return password && confirm && password !== confirm ? { passwordMismatch: true } : null;
}

@Component({
  selector: 'app-sign-up',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule,
  ],
  templateUrl: './sign-up.component.html',
  styleUrl: './sign-up.component.scss',
})
export class SignUpComponent {
  readonly signedUp = output<User>();

  protected hidePassword = true;
  protected hideConfirm = true;
  protected readonly loading = signal(false);
  protected errorMessage = '';
  protected readonly RoleEnum = UserRoleEnum;

  private readonly _fb = inject(FormBuilder);
  private readonly _authService = inject(AuthService);

  protected readonly form = this._fb.nonNullable.group(
    {
      name: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      role: [UserRoleEnum.Customer, Validators.required],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required],
    },
    { validators: passwordMatchValidator },
  );

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage = '';

    const { confirmPassword: _, ...request } = this.form.getRawValue();

    this._authService.signUp(request).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.signedUp.emit(response.user);
      },
      error: (err: Error) => {
        this.loading.set(false);
        this.errorMessage = err.message;
      },
    });
  }
}
