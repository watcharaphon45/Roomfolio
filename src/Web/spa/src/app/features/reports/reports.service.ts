import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ReportSummary {
  totalUnits: number;
  settlementCount: number;
  totalPaidToOwners: number;
  agencyCashHeld: number;
  overdueCount: number;
  overdueAmount: number;
  expiringLeasesCount: number;
}

export interface ReportItem {
  id: string;
  title: string;
  desc: string;
  format: string;
  endpoint: string;
  defaultFilename: string;
}

@Injectable({
  providedIn: 'root'
})
export class ReportsService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/reports`;

  getSummary(): Observable<ReportSummary> {
    return this.http.get<ReportSummary>(`${this.apiUrl}/summary`).pipe(
      catchError(err => {
        console.warn('API getSummary error, falling back:', err);
        return of({
          totalUnits: 6,
          settlementCount: 3,
          totalPaidToOwners: 95500,
          agencyCashHeld: 3350,
          overdueCount: 0,
          overdueAmount: 0,
          expiringLeasesCount: 1
        });
      })
    );
  }

  downloadReport(reportPath: string): Observable<Blob> {
    const url = reportPath.startsWith('http') ? reportPath : `${this.apiUrl}${reportPath.startsWith('/') ? '' : '/'}${reportPath}`;
    return this.http.get(url, {
      responseType: 'blob'
    });
  }

  downloadMaster(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/consolidated/export-csv`, {
      responseType: 'blob'
    });
  }

  saveBlob(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }
}
