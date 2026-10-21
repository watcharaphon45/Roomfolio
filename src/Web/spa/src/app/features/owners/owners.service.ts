import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

export interface OwnerItem {
  id: string;
  initial: string;
  name: string;
  phone: string;
  email: string;
  unitsCount: number;
  feeTerms: string;
  collectionMethod: string;
  bankAccount: string;
  pendingSettlement: number;
}

@Injectable({
  providedIn: 'root'
})
export class OwnersService {
  getOwners(): Observable<OwnerItem[]> {
    return of([
      { id: '1', initial: 'ส', name: 'คุณสมชาย อัครเดช', phone: '081-445-9988', email: 'somchai@email.com', unitsCount: 3, feeTerms: '5% ของค่าเช่ารับจริง', collectionMethod: 'หักจาก Settlement', bankAccount: 'กสิกรไทย ··· 4821', pendingSettlement: 88750 },
      { id: '2', initial: 'ภ', name: 'คุณภัทรินทร์ วงศ์สว่าง', phone: '089-223-1122', email: 'patarin@email.com', unitsCount: 2, feeTerms: '5% ของค่าเช่ารับจริง', collectionMethod: 'หักจาก Settlement', bankAccount: 'ไทยพาณิชย์ ··· 9104', pendingSettlement: 64600 },
      { id: '3', initial: 'S', name: 'Mrs. Sarah Jenkins', phone: '+66 82 555 1204', email: 'sarah.j@expat.com', unitsCount: 1, feeTerms: '฿ 2,000 ต่อห้องคงที่', collectionMethod: 'ออกใบแจ้งหนี้แยก', bankAccount: 'กรุงเทพ ··· 3312', pendingSettlement: 39100 }
    ]);
  }
}
