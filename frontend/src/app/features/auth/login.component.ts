import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { AuthService } from '../../core/services/auth.service';
import { getValidationMessage } from '../../shared/validators/form-error.util';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ButtonModule, CardModule, DividerModule, InputTextModule, PasswordModule, ProgressSpinnerModule, ReactiveFormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly messageService = inject(MessageService);

  readonly isLoading = signal(false);
  readonly errorMessage = signal('');
  readonly form = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  validationMessage(controlName: 'email' | 'password', label: string): string {
    return getValidationMessage(this.form.controls[controlName], label);
  }

  submit(): void {
    if (this.form.invalid || this.isLoading()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');

    this.authService.login(this.form.getRawValue()).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Signed in', detail: 'Locations synced successfully.' });
        void this.router.navigate(['/purchase']);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(this.getErrorMessage(error));
        this.isLoading.set(false);
      },
      complete: () => this.isLoading.set(false)
    });
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    if (typeof error.error === 'string' && error.error.trim().length > 0) {
      return error.error;
    }

    if (typeof error.error?.message === 'string' && error.error.message.trim().length > 0) {
      return error.error.message;
    }

    return 'Login failed. Please check your credentials.';
  }
}
