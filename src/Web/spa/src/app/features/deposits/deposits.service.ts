import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, map, of } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DepositTransactionItem {
  id: string;
  type: number;
  typeName: string;
  amount: number;
  date: string;
  rawDate: string;
  description: string;
  referenceNo?: string;
  note?: string;
}

export interface DepositItem {
  id: string;
  leaseId: string;
  leaseNo: string;
  unit: string;
  tenant: string;
  holderId: number;
  holder: string;
  initialAmount: number;
  deducted: number;
  refunded: number;
  balance: number;
  statusId: number;
  status: string;
  receivedDate?: string;
  bankAccountDetails?: string;
  note?: string;
  transactions?: DepositTransactionItem[];
}

export interface DepositsResponse {
  totalCount: number;
  totalInitialAmount: number;
  agencyHeldAmount: number;
  ownerHeldAmount: number;
  totalDeductedAmount: number;
  totalBalance: number;
  items: DepositItem[];
}

export interface AddTransactionPayload {
  type: number; // 2: Deduction, 3: Refund
  amount: number;
  transactionDate?: string;
  description: string;
  referenceNo?: string;
  note?: string;
}

@Injectable({
  providedIn: 'root'
})
export class DepositsService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/deposits`;

  getDepositsData(search?: string, holder?: number, status?: number): Observable<DepositsResponse> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (holder !== undefined && holder !== null) params = params.set('holder', holder.toString());
    if (status !== undefined && status !== null) params = params.set('status', status.toString());

    return this.http.get<DepositsResponse>(this.apiUrl, { params }).pipe(
      catchError(err => {
        console.warn('API getDepositsData error, falling back:', err);
        const fallbackItems: DepositItem[] = [
          {
            id: '1',
            leaseId: 'l1',
            leaseNo: 'LS-2025-081',
            unit: 'Noble Ploenchit #1402',
            tenant: 'คุณกิตติศักดิ์ พูลสวัสดิ์',
            holderId: 1,
            holder: 'Owner (เจ้าของถือ)',
            initialAmount: 76000,
            deducted: 0,
            refunded: 0,
            balance: 76000,
            statusId: 1,
            status: 'ถือครองอยู่',
            bankAccountDetails: 'โอนตรงเข้าบัญชีเจ้าของห้อง'
          },
          {
            id: '2',
            leaseId: 'l2',
            leaseNo: 'LS-2026-012',
            unit: 'Ashton Asoke #2208',
            tenant: 'Mr. David Miller',
            holderId: 2,
            holder: 'Agency (นายหน้าถือ)',
            initialAmount: 90000,
            deducted: 0,
            refunded: 0,
            balance: 90000,
            statusId: 1,
            status: 'ถือครองอยู่',
            bankAccountDetails: 'KBANK - บัญชีเงินประกันบริษัท Roomfolio'
          },
          {
            id: '3',
            leaseId: 'l3',
            leaseNo: 'LS-2026-034',
            unit: 'Ideo Q Sukhumvit 36 #0915',
            tenant: 'คุณนพวรรณ ชัยเจริญ',
            holderId: 2,
            holder: 'Agency (นายหน้าถือ)',
            initialAmount: 44000,
            deducted: 2500,
            refunded: 0,
            balance: 41500,
            statusId: 2,
            status: 'หักชดเชยบางส่วน',
            bankAccountDetails: 'KBANK - บัญชีเงินประกันบริษัท Roomfolio'
          }
        ];

        return of({
          totalCount: fallbackItems.length,
          totalInitialAmount: 210000,
          agencyHeldAmount: 131500,
          ownerHeldAmount: 76000,
          totalDeductedAmount: 2500,
          totalBalance: 207500,
          items: fallbackItems
        });
      })
    );
  }

  getDeposits(): Observable<DepositItem[]> {
    return this.getDepositsData().pipe(map(res => res.items));
  }

  addTransaction(depositId: string, payload: AddTransactionPayload): Observable<any> {
    return this.http.post(`${this.apiUrl}/${depositId}/transactions`, payload);
  }

  deleteDeposit(id: string): Observable<{ success: boolean }> {
    return this.http.delete<{ success: boolean }>(`${this.apiUrl}/${id}`);
  }
}
