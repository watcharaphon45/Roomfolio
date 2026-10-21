import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

export interface OwnerSettlementItem {
  id: string;
  owner: string;
  period: string;
  unitsCount: number;
  gross: number;
  expense: number;
  fee: number;
  net: number;
  status: string;
  statusBadge: string;
}

export interface SettlementPipelineSummary {
  draftCount: number;
  reviewCount: number;
  approvedCount: number;
  paidCount: number;
  settlements: OwnerSettlementItem[];
}

@Injectable({
  providedIn: 'root'
})
export class OwnerSettlementsService {
  getSettlementsData(): Observable<SettlementPipelineSummary> {
    return of({
      draftCount: 3,
      reviewCount: 5,
      approvedCount: 12,
      paidCount: 48,
      settlements: [
        { id: '1', owner: 'คุณสมชาย อัครเดช', period: '01 - 31 ต.ค. 2026', unitsCount: 3, gross: 95000, expense: 1500, fee: 4750, net: 88750, status: 'รอตรวจทาน', statusBadge: 'badge-amber' },
        { id: '2', owner: 'คุณภัทรินทร์ วงศ์สว่าง', period: '01 - 31 ต.ค. 2026', unitsCount: 2, gross: 68000, expense: 0, fee: 3400, net: 64600, status: 'อนุมัติแล้ว', statusBadge: 'badge-blue' },
        { id: '3', owner: 'Mrs. Sarah Jenkins', period: '01 - 31 ต.ค. 2026', unitsCount: 1, gross: 42000, expense: 800, fee: 2100, net: 39100, status: 'โอนแล้วเสร็จ', statusBadge: 'badge-emerald' }
      ]
    });
  }
}
