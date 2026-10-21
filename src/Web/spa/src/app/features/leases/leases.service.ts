import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

export interface LeaseItem {
  id: string;
  leaseNumber: string;
  unit: string;
  tenant: string;
  startDate: string;
  endDate: string;
  dueDay: number;
  monthlyRent: number;
  paymentMode: 'direct_to_owner' | 'through_agent';
  deposit: number;
  status: string;
}

@Injectable({
  providedIn: 'root'
})
export class LeasesService {
  getLeases(): Observable<LeaseItem[]> {
    return of([
      { id: '1', leaseNumber: 'LS-2025-081', unit: 'Noble Ploenchit #1402', tenant: 'คุณกิตติศักดิ์ พ.', startDate: '01 พ.ย. 2025', endDate: '31 ต.ค. 2026', dueDay: 1, monthlyRent: 38000, paymentMode: 'direct_to_owner', deposit: 76000, status: 'ใกล้หมดอายุ' },
      { id: '2', leaseNumber: 'LS-2026-012', unit: 'Ashton Asoke #2208', tenant: 'Mr. David Miller', startDate: '15 ก.พ. 2026', endDate: '14 ก.พ. 2027', dueDay: 5, monthlyRent: 45000, paymentMode: 'through_agent', deposit: 90000, status: 'ใช้งานอยู่' },
      { id: '3', leaseNumber: 'LS-2026-034', unit: 'Ideo Q #0915', tenant: 'คุณนพวรรณ ช.', startDate: '01 มิ.ย. 2026', endDate: '31 พ.ค. 2027', dueDay: 1, monthlyRent: 22000, paymentMode: 'through_agent', deposit: 44000, status: 'ใช้งานอยู่' }
    ]);
  }
}
