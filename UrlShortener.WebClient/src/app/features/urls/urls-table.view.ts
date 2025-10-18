import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ShortUrlView } from '../../models/short-url.model';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-urls-table-view',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, DatePipe],
  templateUrl: './urls-table.view.html',
  styleUrls: ['./urls-table.view.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UrlsTableViewComponent {
  @Input() urls: ShortUrlView[] = [];
  @Input() isLoading = false;
  @Input() isCreating = false;
  @Input() deletingId: number | null = null;
  @Input() error?: string;
  @Input() newUrl = '';
  @Input() userName: string | null = null;
  @Input() isAdmin = false;                    

  @Output() newUrlChange = new EventEmitter<string>();
  @Output() create = new EventEmitter<void>();
  @Output() delete = new EventEmitter<number>();
  @Output() copy = new EventEmitter<string>();
  @Output() logout = new EventEmitter<void>();

  get isAuthed() { return !!this.userName; } 

  aboutUrl = `${environment.apiBaseUrl}/About`;

  trackById = (_: number, x: ShortUrlView) => x.id;
}