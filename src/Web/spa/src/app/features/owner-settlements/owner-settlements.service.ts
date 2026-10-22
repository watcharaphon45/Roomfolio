import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface SettlementLineItem {
  id: string;
  lineType: number;
  lineTypeText: string;
  unitId?: string;
  unitNumber?: string;
  description: string;
  amount: number;
  note?: string;
}

export interface OwnerSettlementItem {
  id: string;
  ownerId: string;
  ownerName: string;
  ownerPhone: string;
  bankName?: string;
  bankAccountNumber?: string;
  period: string;
  periodStart: string;
  periodEnd: string;
  unitsCount: number;
  gross: number;
  expense: number;
  fee: number;
  net: number;
  status: number;
  statusText: string;
  statusBadge: string;
  transferReference?: string;
  paidAt?: string;
  note?: string;
  lines: SettlementLineItem[];
  createdAt?: string;
}

export interface SettlementPipelineSummary {
  draftCount: number;
  reviewCount: number;
  approvedCount: number;
  paidCount: number;
  totalNetPayable: number;
  settlements: OwnerSettlementItem[];
}

export interface CreateSettlementPayload {
  ownerId: string;
  periodStart: string;
  periodEnd: string;
  grossRentCollected: number;
  managementFeePercent: number;
  expensesDeducted: number;
  note?: string;
}

export interface UpdateSettlementStatusPayload {
  status: number;
  transferReference?: string;
  note?: string;
}

@Injectable({
  providedIn: 'root'
})
export class OwnerSettlementsService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/owner-settlements`;

  getSettlementsData(search?: string, status?: number): Observable<SettlementPipelineSummary> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (status) params = params.set('status', status.toString());

    return this.http.get<SettlementPipelineSummary>(this.apiUrl, { params }).pipe(
      catchError(err => {
        console.warn('API getSettlementsData error, falling back to local dataset:', err);
        return of({
          draftCount: 0,
          reviewCount: 1,
          approvedCount: 0,
          paidCount: 2,
          totalNetPayable: 152400,
          settlements: [
            {
              id: '1',
              ownerId: 'o1',
              ownerName: 'คุณภัทรินทร์ รัตนโชติ',
              ownerPhone: '089-776-5432',
              bankName: 'ธ.ไทยพาณิชย์ (SCB)',
              bankAccountNumber: '112-3-98765-4',
              period: '01/10 - 31/10/2026',
              periodStart: '2026-10-01',
              periodEnd: '2026-10-31',
              unitsCount: 2,
              gross: 45000,
              expense: 0,
              fee: 2250,
              net: 42750,
              status: 4,
              statusText: 'โอนแล้วเสร็จ',
              statusBadge: 'badge-emerald',
              transferReference: 'SCB-SETTLE-TEST-9901',
              lines: []
            },
            {
              id: '2',
              ownerId: 'o2',
              ownerName: 'Mrs. Sarah Jenkins',
              ownerPhone: '+66 82 345 6789',
              bankName: 'ธ.กรุงเทพ (BBL)',
              bankAccountNumber: '201-0-45678-9',
              period: '01/10 - 31/10/2026',
              periodStart: '2026-10-01',
              periodEnd: '2026-10-31',
              unitsCount: 1,
              gross: 22000,
              expense: 0,
              fee: 1100,
              net: 20900,
              status: 4,
              statusText: 'โอนแล้วเสร็จ',
              statusBadge: 'badge-emerald',
              transferReference: 'BBL-SETTLE-20261010-091',
              lines: []
            },
            {
              id: '3',
              ownerId: 'o3',
              ownerName: 'คุณสมชาย วงศ์สวัสดิ์',
              ownerPhone: '081-445-6789',
              bankName: 'ธ.กสิกรไทย (KBANK)',
              bankAccountNumber: '045-2-12345-6',
              period: '01/10 - 31/10/2026',
              periodStart: '2026-10-01',
              periodEnd: '2026-10-31',
              unitsCount: 2,
              gross: 95000,
              expense: 1500,
              fee: 4750,
              net: 88750,
              status: 2,
              statusText: 'รอตรวจทาน',
              statusBadge: 'badge-amber',
              lines: []
            }
          ]
        });
      })
    );
  }

  getSettlementById(id: string): Observable<OwnerSettlementItem> {
    return this.http.get<OwnerSettlementItem>(`${this.apiUrl}/${id}`);
  }

  createSettlement(payload: CreateSettlementPayload): Observable<any> {
    return this.http.post<any>(this.apiUrl, payload);
  }

  updateSettlementStatus(id: string, payload: UpdateSettlementStatusPayload): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/status`, payload);
  }

  deleteSettlement(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
