import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

export interface OrganizationSettings {
  companyName: string;
  taxId: string;
  defaultCurrency: string;
  timezone: string;
  planName: string;
  planPrice: number;
  roomCount: number;
  roomLimit: number;
}

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  getSettings(): Observable<OrganizationSettings> {
    return of({
      companyName: 'Prime Living Agency Co., Ltd.',
      taxId: '0105565012345',
      defaultCurrency: 'THB (฿)',
      timezone: 'Asia/Bangkok (UTC+07:00)',
      planName: 'Pro Plan',
      planPrice: 799,
      roomCount: 184,
      roomLimit: 200
    });
  }
}
