import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

export interface DepositItem {
  id: string;
  unit: string;
  leaseNo: string;
  tenant: string;
  holder: string;
  initialAmount: number;
  deducted: number;
  balance: number;
  status: string;
}

@Injectable({
  providedIn: 'root'
})
export class DepositsService {
  getDeposits(): Observable<DepositItem[]> {
    return of([
      { id: '1', unit: 'Noble Ploenchit #1402', leaseNo: 'LS-2025-081', tenant: 'คุณกิตติศักดิ์ พ.', holder: 'Owner (เจ้าของถือ)', initialAmount: 76000, deducted: 0, balance: 76000, status: 'ถือครองอยู่' },
      { id: '2', unit: 'Ashton Asoke #2208', leaseNo: 'LS-2026-012', tenant: 'Mr. David Miller', holder: 'Agency (นายหน้าถือ)', initialAmount: 90000, deducted: 0, balance: 90000, status: 'ถือครองอยู่' },
      { id: '3', unit: 'Ideo Q #0915', leaseNo: 'LS-2026-034', tenant: 'คุณนพวรรณ ช.', holder: 'Agency (นายหน้าถือ)', initialAmount: 44000, deducted: 2500, balance: 41500, status: 'หักชดเชยบางส่วน' }
    ]);
  }
}
