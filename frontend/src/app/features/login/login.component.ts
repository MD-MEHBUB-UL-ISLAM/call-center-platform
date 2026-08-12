import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  email = signal('rafi.agent@example.com');
  password = signal('Password123!');
  errorMessage = signal<string | null>(null);
  isSubmitting = signal(false);

  constructor(private auth: AuthService, private router: Router) {}

  submit(): void {
    this.errorMessage.set(null);
    this.isSubmitting.set(true);

    this.auth.login(this.email(), this.password()).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.router.navigateByUrl('/dashboard');
      },
      error: () => {
        this.isSubmitting.set(false);
        this.errorMessage.set('Invalid email or password.');
      }
    });
  }
}
