import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <!-- Backdrop -->
    <div class="fixed inset-0 z-40 bg-slate-900/60 backdrop-blur-sm"></div>

    <!-- Modal -->
    <div class="fixed inset-0 z-50 grid place-items-center p-4">
      <div class="w-full max-w-md rounded-2xl bg-white dark:bg-slate-800 shadow-xl ring-1 ring-slate-200 dark:ring-slate-700">
        <div class="p-6">
          <h2 class="text-xl font-semibold tracking-tight mb-4 text-slate-900 dark:text-slate-100">
            Login
          </h2>

          <form (ngSubmit)="submit()" class="space-y-3">
            <div>
              <label class="block text-sm text-slate-600 dark:text-slate-300 mb-1">Username</label>
              <input [(ngModel)]="username" name="username" required
                     class="w-full rounded-lg border border-slate-300 dark:border-slate-700 
                            bg-white dark:bg-slate-900 text-slate-900 dark:text-white
                            placeholder-slate-400 dark:placeholder-slate-500
                            px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-400"
                     placeholder="e.g. admin" />
            </div>

            <div>
              <label class="block text-sm text-slate-600 dark:text-slate-300 mb-1">Password</label>
              <input [(ngModel)]="password" name="password" type="password" required
                     class="w-full rounded-lg border border-slate-300 dark:border-slate-700 
                            bg-white dark:bg-slate-900 text-slate-900 dark:text-white
                            placeholder-slate-400 dark:placeholder-slate-500
                            px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-400"
                     placeholder="••••••••" />
            </div>

            <button type="submit"
                    class="w-full inline-flex items-center justify-center rounded-lg px-3 py-2 text-sm font-medium
                           bg-indigo-600 text-white hover:bg-indigo-700 transition">
              Login
            </button>

            <p *ngIf="error" class="text-sm text-red-600 mt-2">{{ error }}</p>
          </form>

          <p class="mt-3 text-sm text-slate-500 dark:text-slate-400">
            Try <code class="px-1 py-0.5 rounded bg-slate-100 dark:bg-slate-700">admin / admin</code>
            or <code class="px-1 py-0.5 rounded bg-slate-100 dark:bg-slate-700">user / user</code>.
          </p>

          <div class="mt-4 flex justify-center">
            <button type="button" class="text-sm text-slate-500 hover:underline"
                    (click)="close()" aria-label="Close login modal">
              Cancel
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class LoginComponent {
  username = '';
  password = '';
  error: string | null = null;

  constructor(private auth: AuthService, private router: Router) {}

  submit() {
    this.error = null;
    this.auth.login(this.username, this.password).subscribe({
      next: () => this.router.navigateByUrl('/'), 
      error: () => this.error = 'Invalid credentials'
    });
  }

  close() {
    if (window.history.length > 1) history.back();
    else this.router.navigateByUrl('/');
  }
}