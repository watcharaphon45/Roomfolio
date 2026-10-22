import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';

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
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/dashboard/summary`;

  getDashboardData(): Observable<DashboardMetrics> {
    return this.http.get<DashboardMetrics>(this.apiUrl).pipe(
      catchError(err => {
        console.warn('API getDashboardData error, falling back to local fallback:', err);
        return of({
          totalUnits: 6,
          occupancyRate: 66.7,
          availableUnits: 2,
          agencyCashHeld: 0,
          cashHeldCount: 2,
          ownersPending: 1,
          rentChargedThisMonth: 105000,
          rentCollectedPct: 100,
          rentOutstanding: 0,
          agencyFeeReceivable: 3350,
          agencyFeeCount: 2,
          agencyFeeDeductible: 4750,
          directModeCount: 0,
          directModePct: 0,
          directModeAmount: 0,
          throughAgentCount: 2,
          throughAgentPct: 100,
          throughAgentAmount: 67000,
          expiringLeases: [
            { unitNumber: 'Main #2208', project: 'Ashton Asoke', tenant: 'Mr. David Miller', rent: 45000, daysLeft: 115, endDate: '14 Feb 2027' },
            { unitNumber: 'B #0915', project: 'Ideo Q Sukhumvit 36', tenant: 'คุณนพวรรณ ชัยเจริญ', rent: 22000, daysLeft: 221, endDate: '31 May 2027' }
          ],
          settlementPreviews: [
            {
              ownerName: 'Mrs. Sarah Jenkins',
              bankAccount: 'ธ.กรุงเทพ (BBL) ··· 78-9',
              unitsCount: 1,
              grossRent: 22000,
              expenses: 0,
              managementFee: 1100,
              netPayable: 20900,
              statusLabel: 'โอนแล้ว',
              statusClass: 'badge-emerald'
            },
            {
              ownerName: 'คุณภัทรินทร์ รัตนโชติ',
              bankAccount: 'ธ.ไทยพาณิชย์ (SCB) ··· 65-4',
              unitsCount: 2,
              grossRent: 45000,
              expenses: 0,
              managementFee: 2250,
              netPayable: 42750,
              statusLabel: 'โอนแล้ว',
              statusClass: 'badge-emerald'
            },
            {
              ownerName: 'คุณสมชาย วงศ์สวัสดิ์',
              bankAccount: 'ธ.กสิกรไทย (KBANK) ··· 45-6',
              unitsCount: 2,
              grossRent: 95000,
              expenses: 1500,
              managementFee: 4750,
              netPayable: 88750,
              statusLabel: 'รอตรวจสอบ',
              statusClass: 'badge-amber'
            }
          ]
        });
      })
    );
  }
}
