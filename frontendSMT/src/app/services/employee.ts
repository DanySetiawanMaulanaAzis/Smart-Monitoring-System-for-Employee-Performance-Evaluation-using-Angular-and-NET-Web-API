import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class Employee {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // 🔹 Dashboard
  getDashboard(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/employee/dashboard`);
  }

  // 🔹 Daily summary
  getDailySummary(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/employee/daily-summary`);
  }

  // 🔹 Produk
  getProducts(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/employee/products`);
  }

  // 🔹 Semua WorkLogs (userId otomatis dari token JWT di backend)
  getWorkLogs(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/employee`);
  }

  // 🔹 Buat WorkLog baru
  createWorkLog(workLog: any): Observable<number> {
    return this.http.post<number>(`${this.apiUrl}/employee`, workLog);
  }

  // 🔹 Update WorkLog
  updateWorkLog(workLog: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/employee`, workLog);
  }

  // 🔹 Hapus WorkLog
  deleteWorkLog(workLogId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/employee/${workLogId}`);
  }

  // 🔹 Chart data (tanpa userId, backend ambil dari JWT)
  getWorkLogChart(startDate: string, endDate: string): Observable<any> {
    return this.http.get<any>(
      `${this.apiUrl}/employee/worklog/chart?startDate=${startDate}&endDate=${endDate}`
    );
  }

  // 🔹 Update totalTime di WorkLog
  updateTotalTime(workLogId: number, elapsedSeconds: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/employee/update-totaltime`, {
      workLogId,
      elapsedSeconds,
    });
  }
}
