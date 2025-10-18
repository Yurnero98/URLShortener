import { Injectable } from '@angular/core';
import { BehaviorSubject, catchError, EMPTY, tap } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment'; 

export type Role = 'Admin' | 'User';

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(private http: HttpClient) {}

  private userSubject = new BehaviorSubject<string | null>(null);
  user$ = this.userSubject.asObservable();

  private roleSubject = new BehaviorSubject<Role | null>(null);
  role$ = this.roleSubject.asObservable();

  get user(): string | null { return this.userSubject.value; }
  get role(): Role | null { return this.roleSubject.value; }
  isAuthenticated(): boolean { return !!this.role; }
  isAdmin(): boolean { return this.role === 'Admin'; }

  private readonly API = `${environment.apiBaseUrl}/api`;

  login(userName: string, password: string) {
    return this.http.post<{ user: string; role: Role }>(
      `${this.API}/auth/login`,
      { userName, password },
      { withCredentials: true }              
    ).pipe(
      tap(r => {
        this.userSubject.next(r.user);
        this.roleSubject.next(r.role);
      })
    );
  }

  logout() {
    return this.http.post(
      `${this.API}/auth/logout`,
      null,
      { withCredentials: true }   
    ).pipe(tap(() => {
      this.userSubject.next(null);
      this.roleSubject.next(null);
    }));
  }

  refreshMe() {
    return this.http.get<{ name: string; roles: string[] }>(
      `${this.API}/auth/me`,
      { withCredentials: true }
    ).pipe(
      tap(r => {
        if (r?.name) {
          this.userSubject.next(r.name);
          this.roleSubject.next(
            r.roles?.includes('Admin') ? 'Admin' :
            r.roles?.includes('User')  ? 'User'  : null
          );
        } else {
          this.userSubject.next(null);
          this.roleSubject.next(null);
        }
      }),
      catchError(() => {
        this.userSubject.next(null);
        this.roleSubject.next(null);
        return EMPTY;
      })
    );
  }
}