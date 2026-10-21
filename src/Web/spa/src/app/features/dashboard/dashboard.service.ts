import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

export interface DashboardMetrics {
  totalUnits: number;
  occupancyRate: number;
  availableUnits: number;
  agencyCashHeld: number;
  cashHeldCount: number;
  ownersPending: number;
  rentChargedThisMonth: number;
  rentCollectedPct: number;
  rentOutstanding: number;
  agencyFeeReceivable: number;
  agencyFeeCount: number;
  agencyFeeDeductible: number;
  directModeCount: number;
  directModePct: number;
  directModeAmount: number;
  throughAgentCount: number;
  throughAgentPct: number;
  throughAgentAmount: number;
  expiringLeases: Array<{
    unitNumber: string;
    project: string;
    tenant: string;
    rent: number;
    daysLeft: number;
    endDate: string;
  }>;
  settlementPreviews: Array<{
    ownerName: string;
    bankAccount: string;
    unitsCount: number;
    grossRent: number;
    expenses: number;
    managementFee: number;
    netPayable: number;
    statusLabel: string;
    statusClass: string;
  }>;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  getDashboardData(): Observable<DashboardMetrics> {
    return of({
      totalUnits: 184,
      occupancyRate: 95.6,
      availableUnits: 8,
      agencyCashHeld: 428500,
      cashHeldCount: 64,
      ownersPending: 12,
      rentChargedThisMonth: 2800000,
      rentCollectedPct: 87.5,
      rentOutstanding: 350000,
      agencyFeeReceivable: 135000,
      agencyFeeCount: 18,
      agencyFeeDeductible: 92000,
      directModeCount: 120,
      directModePct: 65,
      directModeAmount: 1620000,
      throughAgentCount: 64,
      throughAgentPct: 35,
      throughAgentAmount: 830000,
      expiringLeases: [
        { unitNumber: 'Noble #1402', project: 'Noble Ploenchit', tenant: 'คุณกิตติศักดิ์ พ.', rent: 38000, daysLeft: 12, endDate: '02 พ.ย. 2026' },
        { unitNumber: 'Ashton #2208', project: 'Ashton Asoke', tenant: 'Mr. David Miller', rent: 45000, daysLeft: 18, endDate: '08 พ.ย. 2026' },
        { unitNumber: 'Ideo #0915', project: 'Ideo Q Sukhumvit 36', tenant: 'คุณนพวรรณ ช.', rent: 22000, daysLeft: 27, endDate: '17 พ.ย. 2026' },
        { unitNumber: 'Rhythm #1104', project: 'Rhythm Ekkamai', tenant: 'คุณธนัท ม.', rent: 28000, daysLeft: 34, endDate: '24 พ.ย. 2026' }
      ],
      settlementPreviews: [
        {
          ownerName: 'คุณสมชาย อัครเดช',
          bankAccount: 'กสิกรไทย ··· 4821',
          unitsCount: 3,
          grossRent: 95000,
          expenses: 1500,
          managementFee: 4750,
          netPayable: 88750,
          statusLabel: 'รอตรวจสอบ',
          statusClass: 'badge-amber'
        },
        {
          ownerName: 'คุณภัทรินทร์ วงศ์สว่าง',
          bankAccount: 'ไทยพาณิชย์ ··· 9104',
          unitsCount: 2,
          grossRent: 68000,
          expenses: 0,
          managementFee: 3400,
          netPayable: 64600,
          statusLabel: 'อนุมัติแล้ว',
          statusClass: 'badge-blue'
        },
        {
          ownerName: 'Mrs. Sarah Jenkins',
          bankAccount: 'กรุงเทพ ··· 3312',
          unitsCount: 1,
          grossRent: 42000,
          expenses: 800,
          managementFee: 2100,
          netPayable: 39100,
          statusLabel: 'โอนแล้ว',
          statusClass: 'badge-emerald'
        }
      ]
    });
  }
}
