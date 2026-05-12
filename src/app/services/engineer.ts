import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class Engineer {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/engineer`;

  getCompletedTasks(startDate?: string, endDate?: string): Observable<any> {
    const params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;

    return this.http.get<any>(`${this.apiUrl}/completed-tasks`, { params });
  }

  getUserPerformance(startDate?: string, endDate?: string): Observable<any> {
    const params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;

    return this.http.get<any>(`${this.apiUrl}/user-performance`, { params });
  }

  getUserPerformanceTable(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/user-performance-table`);
  }
  
}
