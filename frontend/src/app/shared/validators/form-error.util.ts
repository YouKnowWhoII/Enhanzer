import { AbstractControl } from '@angular/forms';

export function getValidationMessage(control: AbstractControl | null, label: string): string {
  if (!control || !control.errors || !(control.dirty || control.touched)) {
    return '';
  }

  if (control.hasError('required')) {
    return `${label} is required.`;
  }

  if (control.hasError('email')) {
    return 'Enter a valid email address.';
  }

  if (control.hasError('min')) {
    return `${label} must be greater than zero.`;
  }

  if (control.hasError('max')) {
    return `${label} is too high.`;
  }

  return `${label} is invalid.`;
}
