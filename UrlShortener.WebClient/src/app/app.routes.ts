import { Routes } from '@angular/router';
import { UrlsTableContainerComponent } from './features/urls/urls-table.container';
import { LoginComponent } from './features/login/login.component';

export const routes: Routes = [
  { path: '', component: UrlsTableContainerComponent },
  { path: 'login', component: LoginComponent },
  { path: '**', redirectTo: '' },
  { path: 'login', component: LoginComponent, outlet: 'modal' },
];