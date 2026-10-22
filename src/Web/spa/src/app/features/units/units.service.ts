import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface UnitItem {
  id: string;
  projectId: string;
  projectName: string;
  ownerId?: string;
  ownerName?: string;
  ownerPhone?: string;
  unitNumber: string;
  floor: number;
  building?: string;
  bedrooms: number;
  bathrooms: number;
  areaSqM: number;
  targetRentPrice: number;
  status: number;
  statusText: string;
  paymentMode: number;
  paymentModeText: string;
  tenantName?: string;
  note?: string;
  createdAt?: string;
}

export interface CreateUnitPayload {
  projectId: string;
  ownerId?: string | null;
  unitNumber: string;
  floor: number;
  building?: string;
  bedrooms: number;
  bathrooms: number;
  areaSqM: number;
  targetRentPrice: number;
  status: number;
  paymentMode: number;
  tenantName?: string;
  note?: string;
}

export interface ProjectLookup {
  id: string;
  name: string;
}

export interface OwnerLookup {
  id: string;
  fullName: string;
  phone: string;
}

@Injectable({
  providedIn: 'root'
})
export class UnitsService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/units`;

  getUnits(search?: string, status?: number): Observable<UnitItem[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (status !== undefined && status !== null) params = params.set('status', status.toString());

    return this.http.get<UnitItem[]>(this.apiUrl, { params }).pipe(
      catchError(err => {
        console.warn('API getUnits error, falling back to local dataset:', err);
        return of([
          {
            id: '1',
            projectId: 'p1',
            projectName: 'Noble Ploenchit',
            ownerName: 'คุณสมชาย วงศ์สวัสดิ์',
            ownerPhone: '081-445-6789',
            unitNumber: '#1402',
            floor: 14,
            building: 'A',
            bedrooms: 1,
            bathrooms: 1,
            areaSqM: 45.5,
            targetRentPrice: 38000,
            status: 2,
            statusText: 'ติดสัญญาเช่า',
            paymentMode: 1,
            paymentModeText: 'จ่ายตรงเจ้าของ',
            tenantName: 'คุณกิตติศักดิ์ พ.',
            note: 'สัญญาเช่า 1 ปี ถึง 31 ต.ค. 2026'
          },
          {
            id: '2',
            projectId: 'p2',
            projectName: 'Ashton Asoke',
            ownerName: 'คุณภัทรินทร์ รัตนโชติ',
            ownerPhone: '089-776-5432',
            unitNumber: '#2208',
            floor: 22,
            building: 'Main',
            bedrooms: 1,
            bathrooms: 1,
            areaSqM: 48.0,
            targetRentPrice: 45000,
            status: 2,
            statusText: 'ติดสัญญาเช่า',
            paymentMode: 2,
            paymentModeText: 'ผ่านนายหน้า',
            tenantName: 'Mr. David Miller',
            note: 'ผู้เช่าชาวต่างชาติ ค่าเช่าผ่านนายหน้า'
          },
          {
            id: '3',
            projectId: 'p3',
            projectName: 'Ideo Q Sukhumvit 36',
            ownerName: 'Mrs. Sarah Jenkins',
            ownerPhone: '+66 82 345 6789',
            unitNumber: '#0915',
            floor: 9,
            building: 'B',
            bedrooms: 1,
            bathrooms: 1,
            areaSqM: 34.0,
            targetRentPrice: 22000,
            status: 2,
            statusText: 'ติดสัญญาเช่า',
            paymentMode: 2,
            paymentModeText: 'ผ่านนายหน้า',
            tenantName: 'คุณนพวรรณ ช.',
            note: 'ชำระทุกวันที่ 1 ผ่านบัญชีนายหน้า'
          },
          {
            id: '4',
            projectId: 'p4',
            projectName: 'Rhythm Ekkamai',
            ownerName: 'คุณนพพร สุขเกษม',
            ownerPhone: '086-554-1122',
            unitNumber: '#1104',
            floor: 11,
            building: 'Main',
            bedrooms: 1,
            bathrooms: 1,
            areaSqM: 36.5,
            targetRentPrice: 28000,
            status: 1,
            statusText: 'ห้องว่าง',
            paymentMode: 1,
            paymentModeText: 'จ่ายตรงเจ้าของ',
            note: 'ห้องว่าง พร้อมนัดชมห้อง ทำความสะอาดแล้ว'
          }
        ]);
      })
    );
  }

  getUnitById(id: string): Observable<UnitItem> {
    return this.http.get<UnitItem>(`${this.apiUrl}/${id}`);
  }

  createUnit(payload: CreateUnitPayload): Observable<UnitItem> {
    return this.http.post<UnitItem>(this.apiUrl, payload);
  }

  updateUnit(id: string, payload: CreateUnitPayload): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, payload);
  }

  deleteUnit(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getProjects(): Observable<ProjectLookup[]> {
    return this.http.get<ProjectLookup[]>(`${environment.apiUrl}/projects`).pipe(
      catchError(() => of([
        { id: '11111111-1111-1111-1111-111111111111', name: 'Noble Ploenchit' },
        { id: '22222222-2222-2222-2222-222222222222', name: 'Ashton Asoke' },
        { id: '33333333-3333-3333-3333-333333333333', name: 'Ideo Q Sukhumvit 36' },
        { id: '44444444-4444-4444-4444-444444444444', name: 'Rhythm Ekkamai' }
      ]))
    );
  }

  getOwners(): Observable<OwnerLookup[]> {
    return this.http.get<OwnerLookup[]>(`${environment.apiUrl}/owners`).pipe(
      catchError(() => of([
        { id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', fullName: 'คุณสมชาย วงศ์สวัสดิ์', phone: '081-445-6789' },
        { id: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', fullName: 'คุณภัทรินทร์ รัตนโชติ', phone: '089-776-5432' },
        { id: 'cccccccc-cccc-cccc-cccc-cccccccccccc', fullName: 'Mrs. Sarah Jenkins', phone: '+66 82 345 6789' },
        { id: 'dddddddd-dddd-dddd-dddd-dddddddddddd', fullName: 'คุณนพพร สุขเกษม', phone: '086-554-1122' }
      ]))
    );
  }
}
