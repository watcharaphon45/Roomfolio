import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface TenantItem {
  id: string;
  fullName: string;
  phone: string;
  email?: string;
  idCardOrPassport?: string;
  lineId?: string;
  emergencyContact?: string;
  status: string;
  note?: string;
  activeLeasesCount: number;
  currentUnit?: string;
  currentLeaseNumber?: string;
  currentDeposit?: number;
  createdAt?: string;
}

export interface CreateTenantPayload {
  fullName: string;
  phone: string;
  email?: string;
  idCardOrPassport?: string;
  lineId?: string;
  emergencyContact?: string;
  note?: string;
}

export interface UpdateTenantPayload {
  fullName: string;
  phone: string;
  email?: string;
  idCardOrPassport?: string;
  lineId?: string;
  emergencyContact?: string;
  status: string;
  note?: string;
}

@Injectable({
  providedIn: 'root'
})
export class TenantsService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/tenants`;

  getTenants(search?: string): Observable<TenantItem[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);

    return this.http.get<TenantItem[]>(this.apiUrl, { params }).pipe(
      catchError(err => {
        console.warn('API getTenants error, falling back to local dataset:', err);
        return of([
          {
            id: '1',
            fullName: 'คุณกิตติศักดิ์ พูลสวัสดิ์',
            phone: '081-998-1122',
            email: 'kittisak.p@gmail.com',
            status: 'Active',
            activeLeasesCount: 1,
            currentUnit: 'Noble Ploenchit #1402',
            currentLeaseNumber: 'LS-2025-081',
            currentDeposit: 76000
          },
          {
            id: '2',
            fullName: 'Mr. David Miller',
            phone: '+66 84 123 4567',
            email: 'david.m@corporate.com',
            status: 'Active',
            activeLeasesCount: 1,
            currentUnit: 'Ashton Asoke #2208',
            currentLeaseNumber: 'LS-2026-012',
            currentDeposit: 90000
          },
          {
            id: '3',
            fullName: 'คุณนพวรรณ ชัยเจริญ',
            phone: '086-554-3321',
            email: 'nopawan.c@yahoo.com',
            status: 'Active',
            activeLeasesCount: 1,
            currentUnit: 'Ideo Q Sukhumvit 36 #0915',
            currentLeaseNumber: 'LS-2026-034',
            currentDeposit: 44000
          }
        ]);
      })
    );
  }

  getTenantById(id: string): Observable<TenantItem> {
    return this.http.get<TenantItem>(`${this.apiUrl}/${id}`);
  }

  createTenant(payload: CreateTenantPayload): Observable<TenantItem> {
    return this.http.post<TenantItem>(this.apiUrl, payload);
  }

  updateTenant(id: string, payload: UpdateTenantPayload): Observable<TenantItem> {
    return this.http.put<TenantItem>(`${this.apiUrl}/${id}`, payload);
  }

  deleteTenant(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
