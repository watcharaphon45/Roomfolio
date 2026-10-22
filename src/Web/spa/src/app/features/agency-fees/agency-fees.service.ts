import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, map, of } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AgencyFeeItem {
  id: string;
  feeTypeId: number;
  feeType: string;
  unitId?: string;
  unit: string;
  ownerId?: string;
  payer: string;
  period: string;
  calcBasis: string;
  amount: number;
  methodId: number;
  method: string;
  statusId: number;
  status: string;
  invoiceNo?: string;
  collectedAt?: string;
  note?: string;
}

export interface AgencyFeesResponse {
  totalCount: number;
  totalAmount: number;
  collectedAmount: number;
  pendingDeductionAmount: number;
  invoicedAmount: number;
  items: AgencyFeeItem[];
}

export interface CreateAgencyFeePayload {
  unitId: string;
  leaseId?: string | null;
  feeType: number;
  description: string;
  period: string;
  calculationBasis: string;
  amount: number;
  collectionMethod: number;
  status: number;
  invoiceNo?: string;
  note?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AgencyFeesService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/agency-fees`;

  getAgencyFeesData(search?: string, feeType?: number, status?: number, method?: number): Observable<AgencyFeesResponse> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (feeType !== undefined && feeType !== null) params = params.set('feeType', feeType.toString());
    if (status !== undefined && status !== null) params = params.set('status', status.toString());
    if (method !== undefined && method !== null) params = params.set('method', method.toString());

    return this.http.get<AgencyFeesResponse>(this.apiUrl, { params }).pipe(
      catchError(err => {
        console.warn('API getAgencyFeesData error, falling back:', err);
        const fallbackItems: AgencyFeeItem[] = [
          {
            id: '1',
            feeTypeId: 1,
            feeType: 'ค่าบริหารรายเดือน (Monthly Management)',
            unit: 'Noble Ploenchit #1402',
            payer: 'เจ้าของ (คุณสมชาย วงศ์สวัสดิ์)',
            period: 'ต.ค. 2026',
            calcBasis: '5% ของค่าเช่า ฿38,000',
            amount: 1900,
            methodId: 1,
            method: 'หักจาก Settlement',
            statusId: 1,
            status: 'รอนำส่ง/หักยอด'
          },
          {
            id: '2',
            feeTypeId: 2,
            feeType: 'ค่านายหน้าปล่อยเช่า (Leasing Commission)',
            unit: 'Ashton Asoke #2208',
            payer: 'เจ้าของ (คุณภัทรินทร์ รัตนโชติ)',
            period: 'สัญญาใหม่ 1 ปี',
            calcBasis: 'ค่าเช่า 1 เดือน (฿45,000)',
            amount: 45000,
            methodId: 1,
            method: 'หักจาก Settlement',
            statusId: 3,
            status: 'รับเงินแล้ว'
          },
          {
            id: '3',
            feeTypeId: 1,
            feeType: 'ค่าบริหารรายเดือน (Monthly Management)',
            unit: 'Ideo Q Sukhumvit 36 #0915',
            payer: 'เจ้าของ (Mrs. Sarah Jenkins)',
            period: 'ต.ค. 2026',
            calcBasis: 'Fixed ฿2,000 / เดือน',
            amount: 2000,
            methodId: 2,
            method: 'ออก Invoice แยก',
            statusId: 2,
            status: 'ออกใบแจ้งหนี้แล้ว'
          }
        ];

        return of({
          totalCount: fallbackItems.length,
          totalAmount: 48900,
          collectedAmount: 45000,
          pendingDeductionAmount: 1900,
          invoicedAmount: 2000,
          items: fallbackItems
        });
      })
    );
  }

  getAgencyFees(): Observable<AgencyFeeItem[]> {
    return this.getAgencyFeesData().pipe(map(res => res.items));
  }

  createAgencyFee(payload: CreateAgencyFeePayload): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.apiUrl, payload);
  }

  updateFeeStatus(id: string, status: number): Observable<{ success: boolean }> {
    return this.http.put<{ success: boolean }>(`${this.apiUrl}/${id}/status`, { status });
  }

  deleteAgencyFee(id: string): Observable<{ success: boolean }> {
    return this.http.delete<{ success: boolean }>(`${this.apiUrl}/${id}`);
  }
}
