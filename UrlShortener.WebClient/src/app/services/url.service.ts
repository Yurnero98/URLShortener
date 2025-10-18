import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { ShortUrlDto, ShortUrlView } from '../models/short-url.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class UrlService {
  private readonly apiBase = environment.apiBaseUrl;            
  private readonly api = `${this.apiBase}/api/urls`;
  private readonly shortBase = environment.publicBaseUrl;      

  constructor(private http: HttpClient) {}

  private toView = (x: ShortUrlDto): ShortUrlView => ({
    ...x,
    shortUrl: `${this.shortBase.replace(/\/$/, '')}/${x.shortCode}`
  });

  getAll(): Observable<ShortUrlView[]> {
    return this.http
      .get<ShortUrlDto[]>(this.api)                 
      .pipe(map(arr => arr.map(this.toView)));
  }

  get(id: number): Observable<ShortUrlView> {
    return this.http
      .get<ShortUrlDto>(`${this.api}/${id}`)
      .pipe(map(this.toView));
  }

  create(originalUrl: string): Observable<ShortUrlView> {
    return this.http
      .post<ShortUrlDto>(this.api, { originalUrl })
      .pipe(map(this.toView));
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.api}/${id}`);
  }
}