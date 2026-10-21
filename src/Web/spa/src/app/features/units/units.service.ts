import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

export interface UnitItem {
  id: string;
  unitNumber: string;
  project: string;
  type: string;
  area: number;
  owner: string;
  ownership: string;
  status: string;
  rent: number;
}

@Injectable({
  providedIn: 'root'
})
export class UnitsService {
  getUnits(): Observable<UnitItem[]> {
    return of([
      { id: '1', unitNumber: '1402 (ชั้น 14)', project: 'Noble Ploenchit', type: '1 Bed, 1 Bath', area: 48, owner: 'คุณสมชาย อัครเดช', ownership: 'สัดส่วน 100%', status: 'มีผู้เช่า', rent: 38000 },
      { id: '2', unitNumber: '2208 (ชั้น 22)', project: 'Ashton Asoke', type: '2 Bed, 2 Bath', area: 65, owner: 'คุณภัทรินทร์ วงศ์สว่าง', ownership: 'สัดส่วน 100%', status: 'มีผู้เช่า', rent: 45000 },
      { id: '3', unitNumber: '0915 (ชั้น 9)', project: 'Ideo Q Sukhumvit 36', type: 'Studio', area: 32, owner: 'Mrs. Sarah Jenkins', ownership: 'สัดส่วน 100%', status: 'มีผู้เช่า', rent: 22000 },
      { id: '4', unitNumber: '0504 (ชั้น 5)', project: 'Rhythm Ekkamai', type: '1 Bed, 1 Bath', area: 35, owner: 'คุณกิตติศักดิ์ & คุณวิภา', ownership: 'เจ้าของร่วม (50/50)', status: 'ว่าง', rent: 26000 }
    ]);
  }
}
