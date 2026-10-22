import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface OrganizationSettings {
  id?: string;
  companyName: string;
  taxId: string;
  defaultCurrency: string;
  timezone: string;
  planName: string;
  planPrice: number;
  roomCount: number;
  roomLimit: number;
  address?: string;
  phone?: string;
  email?: string;
  updatedAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/settings`;

  getSettings(): Observable<OrganizationSettings> {
    return this.http.get<OrganizationSettings>(this.apiUrl).pipe(
      catchError(err => {
        console.warn('API getSettings error, falling back:', err);
        return of({
          companyName: 'Prime Living Agency Co., Ltd.',
          taxId: '0105565012345',
          defaultCurrency: 'THB (฿)',
          timezone: 'Asia/Bangkok (UTC+07:00)',
          planName: 'Pro Plan',
          planPrice: 799,
          roomCount: 6,
          roomLimit: 200,
          address: '88/1 Sukhumvit 55, Thonglor, Watthana, Bangkok 10110',
          phone: '02-712-9988',
          email: 'contact@primeliving.co.th'
        });
      })
    );
  }

  updateSettings(payload: Partial<OrganizationSettings>): Observable<OrganizationSettings> {
    return this.http.put<OrganizationSettings>(this.apiUrl, payload);
  }

  upgradePlan(planName: string, planPrice: number, roomLimit: number): Observable<OrganizationSettings> {
    return this.http.post<OrganizationSettings>(`${this.apiUrl}/upgrade-plan`, { planName, planPrice, roomLimit });
  }
}
