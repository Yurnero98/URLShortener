import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './auth/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `
    <router-outlet></router-outlet>
    <router-outlet name="modal"></router-outlet>
  `
})
export class AppComponent implements OnInit {
  constructor(private auth: AuthService) {}
  ngOnInit() { this.auth.refreshMe().subscribe(); }
}