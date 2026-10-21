import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

export interface RentChargeItem {
  id: string;
  unit: string;
  period: string;
  tenant: string;
  dueDate: string;
  chargeAmount: number;
  paidAmount: number;
  outstanding: number;
  mode: 'direct_to_owner' | 'through_agent';
  status: string;
  statusClass: string;
}

export interface RentCollectionSummary {
  totalTarget: number;
  totalCollected: number;
  totalOutstanding: number;
  agencyCashHeld: number;
  items: RentChargeItem[];
}

@Injectable({
  providedIn: 'root'
})
export class RentCollectionService {
  getRentCollectionData(): Observable<RentCollectionSummary> {
    return of({
      totalTarget: 2800000,
      totalCollected: 2450000,
      totalOutstanding: 350000,
      agencyCashHeld: 428500,
      items: [
        { id: '1', unit: 'Noble Ploenchit #1402', period: 'ต.ค. 2026', tenant: 'คุณกิตติศักดิ์ พ.', dueDate: '01 ต.ค. 2026', chargeAmount: 38000, paidAmount: 38000, outstanding: 0, mode: 'direct_to_owner', status: 'ชำระครบแล้ว', statusClass: 'badge-emerald' },
        { id: '2', unit: 'Ashton Asoke #2208', period: 'ต.ค. 2026', tenant: 'Mr. David Miller', dueDate: '05 ต.ค. 2026', chargeAmount: 45000, paidAmount: 45000, outstanding: 0, mode: 'through_agent', status: 'เงินอยู่นายหน้า', statusClass: 'badge-emerald' },
        { id: '3', unit: 'Ideo Q #0915', period: 'ต.ค. 2026', tenant: 'คุณนพวรรณ ช.', dueDate: '01 ต.ค. 2026', chargeAmount: 22000, paidAmount: 12000, outstanding: 10000, mode: 'through_agent', status: 'ชำระบางส่วน', statusClass: 'badge-amber' },
        { id: '4', unit: 'Rhythm Ekkamai #1104', period: 'ต.ค. 2026', tenant: 'คุณธนัท ม.', dueDate: '01 ต.ค. 2026', chargeAmount: 28000, paidAmount: 0, outstanding: 28000, mode: 'direct_to_owner', status: 'เกินกำหนด', statusClass: 'badge-rose' }
      ]
    });
  }
}
