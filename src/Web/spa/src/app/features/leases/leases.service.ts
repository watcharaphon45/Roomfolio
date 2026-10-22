import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LeaseItem {
  id: string;
  unitId: string;
  unitNumber: string;
  projectName: string;
  tenantId: string;
  tenantName: string;
  tenantPhone: string;
  leaseNumber: string;
  startDate: string;
  endDate: string;
  rentDueDay: number;
  monthlyRent: number;
  paymentMode: number;
  paymentModeText: string;
  depositAmount: number;
  depositHolder: number;
  depositHolderText: string;
  status: number;
  statusText: string;
  signedAt?: string;
  note?: string;
  createdAt?: string;
}

export interface CreateLeasePayload {
  unitId: string;
  tenantId: string;
  leaseNumber: string;
  startDate: string;
  endDate: string;
  rentDueDay: number;
  monthlyRent: number;
  paymentMode: number;
  depositAmount: number;
  depositHolder: number;
  status: number;
  signedAt?: string;
  note?: string;
}

export interface UpdateLeasePayload {
  unitId: string;
  tenantId: string;
  leaseNumber: string;
  startDate: string;
  endDate: string;
  rentDueDay: number;
  monthlyRent: number;
  paymentMode: number;
  depositAmount: number;
  depositHolder: number;
  status: number;
  signedAt?: string;
  terminatedAt?: string;
  terminationReason?: string;
  note?: string;
}

@Injectable({
  providedIn: 'root'
})
export class LeasesService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/leases`;

  getLeases(search?: string, paymentMode?: number, status?: number): Observable<LeaseItem[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (paymentMode) params = params.set('paymentMode', paymentMode.toString());
    if (status) params = params.set('status', status.toString());

    return this.http.get<LeaseItem[]>(this.apiUrl, { params }).pipe(
      catchError(err => {
        console.warn('API getLeases error, falling back to local dataset:', err);
        return of([
          {
            id: '1',
            unitId: 'u1',
            unitNumber: '#1402',
            projectName: 'Noble Ploenchit',
            tenantId: 't1',
            tenantName: 'คุณกิตติศักดิ์ พูลสวัสดิ์',
            tenantPhone: '081-998-1122',
            leaseNumber: 'LS-2025-081',
            startDate: '2025-11-01',
            endDate: '2026-10-31',
            rentDueDay: 1,
            monthlyRent: 38000,
            paymentMode: 1,
            paymentModeText: 'จ่ายตรงเจ้าของ',
            depositAmount: 76000,
            depositHolder: 1,
            depositHolderText: 'เจ้าของถือ',
            status: 3,
            statusText: 'ใกล้หมดอายุ'
          },
          {
            id: '2',
            unitId: 'u2',
            unitNumber: '#2208',
            projectName: 'Ashton Asoke',
            tenantId: 't2',
            tenantName: 'Mr. David Miller',
            tenantPhone: '+66 84 123 4567',
            leaseNumber: 'LS-2026-012',
            startDate: '2026-02-15',
            endDate: '2027-02-14',
            rentDueDay: 5,
            monthlyRent: 45000,
            paymentMode: 2,
            paymentModeText: 'ผ่านนายหน้า',
            depositAmount: 90000,
            depositHolder: 2,
            depositHolderText: 'นายหน้าถือ',
            status: 2,
            statusText: 'ใช้งานอยู่'
          },
          {
            id: '3',
            unitId: 'u3',
            unitNumber: '#0915',
            projectName: 'Ideo Q Sukhumvit 36',
            tenantId: 't3',
            tenantName: 'คุณนพวรรณ ชัยเจริญ',
            tenantPhone: '086-554-3321',
            leaseNumber: 'LS-2026-034',
            startDate: '2026-06-01',
            endDate: '2027-05-31',
            rentDueDay: 1,
            monthlyRent: 22000,
            paymentMode: 2,
            paymentModeText: 'ผ่านนายหน้า',
            depositAmount: 44000,
            depositHolder: 2,
            depositHolderText: 'นายหน้าถือ',
            status: 2,
            statusText: 'ใช้งานอยู่'
          }
        ]);
      })
    );
  }

  getLeaseById(id: string): Observable<LeaseItem> {
    return this.http.get<LeaseItem>(`${this.apiUrl}/${id}`);
  }

  createLease(payload: CreateLeasePayload): Observable<LeaseItem> {
    return this.http.post<LeaseItem>(this.apiUrl, payload);
  }

  updateLease(id: string, payload: UpdateLeasePayload): Observable<LeaseItem> {
    return this.http.put<LeaseItem>(`${this.apiUrl}/${id}`, payload);
  }

  deleteLease(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
