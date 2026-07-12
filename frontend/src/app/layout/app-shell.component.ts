import { Component, input, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [ButtonModule],
  templateUrl: './app-shell.component.html'
})
export class AppShellComponent {
  readonly title = input.required<string>();
  readonly eyebrow = input('Enhanzer');
  readonly logout = output<void>();
}
