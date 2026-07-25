import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  readonly baseUrl = environment.apiUrl;

  get<T>(path: string) {
    return this.http.get<T>(`${this.baseUrl}${path}`);
  }

  getBlob(path: string) {
    return this.http.get(`${this.baseUrl}${path}`, {
      responseType: 'blob',
      observe: 'response'
    });
  }

  post<T>(path: string, body: unknown) {
    return this.http.post<T>(`${this.baseUrl}${path}`, body);
  }

  postFormData<T>(path: string, formData: FormData) {
    return this.http.post<T>(`${this.baseUrl}${path}`, formData);
  }

  put<T>(path: string, body: unknown) {
    return this.http.put<T>(`${this.baseUrl}${path}`, body);
  }

  patch<T>(path: string, body: unknown = {}) {
    return this.http.patch<T>(`${this.baseUrl}${path}`, body);
  }

  delete<T>(path: string) {
    return this.http.delete<T>(`${this.baseUrl}${path}`);
  }
}
