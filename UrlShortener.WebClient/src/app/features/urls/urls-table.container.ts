import { ChangeDetectionStrategy, Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UrlsTableViewComponent } from './urls-table.view';
import { UrlService } from '../../services/url.service';
import { AuthService } from '../../auth/auth.service';
import { ShortUrlView } from '../../models/short-url.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-urls-table',
  standalone: true,
  imports: [CommonModule, UrlsTableViewComponent],
  template: `
    <app-urls-table-view
      [urls]="urls"
      [isLoading]="isLoading"
      [isCreating]="isCreating"
      [deletingId]="deletingId"
      [error]="error"
      [newUrl]="newUrl"
      [userName]="auth.user$ | async"
      [isAdmin]="(auth.role$ | async) === 'Admin'"
      (newUrlChange)="newUrl = $event"
      (create)="onCreate()"
      (delete)="onDelete($event)"
      (copy)="onCopy($event)"
      (logout)="onLogout()"
    />
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UrlsTableContainerComponent implements OnInit {
  urls: ShortUrlView[] = [];
  newUrl = '';
  error?: string;
  isLoading = true;
  isCreating = false;
  deletingId: number | null = null;

  constructor(
    private svc: UrlService,
    public  auth: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.load(); 
  }

  private load() {
    this.isLoading = true;
    this.cdr.markForCheck();

    this.svc.getAll().subscribe({
      next: list => {
        this.urls = list;
        this.error = undefined;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: err => {
        this.error = this.pickError(err, 'Failed to load URLs');
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  onCreate() {
    if (!this.auth.isAuthenticated()) {
      this.error = 'Please log in to create short URLs.';
      this.cdr.markForCheck();
      return;
    }
    const url = this.newUrl.trim(); if (!url) return;

    this.isCreating = true;
    this.cdr.markForCheck();

    this.svc.create(url).subscribe({
      next: created => {
        this.urls = [created, ...this.urls];
        this.newUrl = '';
        this.error = undefined;
        this.isCreating = false;
        this.cdr.markForCheck();
      },
      error: err => {
        this.error = this.pickError(err, 'Create failed');
        this.isCreating = false;
        this.cdr.markForCheck();
      }
    });
  }

  onDelete(id: number) {
    if (!this.auth.isAuthenticated()) {
      this.error = 'Please log in to delete.';
      this.cdr.markForCheck();
      return;
    }
    if (!confirm('Delete this short URL?')) return;

    this.deletingId = id;
    this.cdr.markForCheck();

    this.svc.delete(id).subscribe({
      next: () => {
        this.urls = this.urls.filter(u => u.id !== id);
        this.deletingId = null;
        this.cdr.markForCheck();
      },
      error: err => {
        this.error = this.pickError(err, 'Delete failed');
        this.deletingId = null;
        this.cdr.markForCheck();
      }
    });
  }

  onCopy(text: string) { navigator.clipboard?.writeText(text); }

  onLogout() {
    console.log('Logout clicked');
    this.auth.logout().subscribe({
      next: () => {
        this.urls = [];
        this.error = undefined;
        this.load(); 
      }
    });
  }

  private pickError(err: any, fallback: string): string {
    if (typeof err?.error === 'string' && err.error) return err.error;
    if (err?.status === 401) return 'Unauthorized — please log in.';
    if (err?.status === 403) return 'Forbidden — only owner or admin can delete.';
    return fallback;
    }
}