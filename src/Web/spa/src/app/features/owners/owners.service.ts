import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface OwnerItem {
  id: string;
  fullName: string;
  phone: string;
  email?: string;
  bankName?: string;
  bankAccountNumber?: string;
  lineId?: string;
  unitsCount: number;
  unitNumbers: string[];
  createdAt?: string;
}

export interface CreateOwnerPayload {
  fullName: string;
  phone: string;
  email?: string;
  bankName?: string;
  bankAccountNumber?: string;
  lineId?: string;
}

@Injectable({
  providedIn: 'root'
})
export class OwnersService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/owners`;

  getOwners(search?: string): Observable<OwnerItem[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);

    return this.http.get<OwnerItem[]>(this.apiUrl, { params }).pipe(
      catchError(err => {
        console.warn('API getOwners error, falling back to local dataset:', err);
        return of([
          {
            id: '1',
            fullName: 'คุณสมชาย วงศ์สวัสดิ์',
            phone: '081-445-6789',
            email: 'somchai.w@gmail.com',
            bankName: 'ธ.กสิกรไทย (KBANK)',
            bankAccountNumber: '045-2-12345-6',
            lineId: 'somchai_condo',
            unitsCount: 2,
            unitNumbers: ['#1402', '#2805']
          },
          {
            id: '2',
            fullName: 'คุณภัทรินทร์ รัตนโชติ',
            phone: '089-776-5432',
            email: 'patarin.r@outlook.com',
            bankName: 'ธ.ไทยพาณิชย์ (SCB)',
            bankAccountNumber: '112-3-98765-4',
            lineId: 'patarin_p',
            unitsCount: 2,
            unitNumbers: ['#2208', '#3502']
          },
          {
            id: '3',
            fullName: 'Mrs. Sarah Jenkins',
            phone: '+66 82 345 6789',
            email: 'sarah.j@globalasset.sg',
            bankName: 'ธ.กรุงเทพ (BBL)',
            bankAccountNumber: '201-0-45678-9',
            lineId: 'sarah_j_bkk',
            unitsCount: 1,
            unitNumbers: ['#0915']
          },
          {
            id: '4',
            fullName: 'คุณนพพร สุขเกษม',
            phone: '086-554-1122',
            email: 'nopporn.s@gmail.com',
            bankName: 'ธ.กรุงไทย (KTB)',
            bankAccountNumber: '008-1-55443-2',
            lineId: 'nopporn_condo',
            unitsCount: 1,
            unitNumbers: ['#1104']
          }
        ]);
      })
    );
  }

  getOwnerById(id: string): Observable<OwnerItem> {
    return this.http.get<OwnerItem>(`${this.apiUrl}/${id}`);
  }

  createOwner(payload: CreateOwnerPayload): Observable<OwnerItem> {
    return this.http.post<OwnerItem>(this.apiUrl, payload);
  }

  updateOwner(id: string, payload: CreateOwnerPayload): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, payload);
  }

  deleteOwner(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
