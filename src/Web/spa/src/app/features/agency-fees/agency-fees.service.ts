import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

export interface AgencyFeeItem {
  id: string;
  feeType: string;
  unit: string;
  payer: string;
  period: string;
  calcBasis: string;
  amount: number;
  method: string;
  status: string;
}

@Injectable({
  providedIn: 'root'
})
export class AgencyFeesService {
  getAgencyFees(): Observable<AgencyFeeItem[]> {
    return of([
      { id: '1', feeType: 'ค่าบริหารรายเดือน (Monthly Management)', unit: 'Noble Ploenchit #1402', payer: 'เจ้าของ (คุณสมชาย)', period: 'ต.ค. 2026', calcBasis: '5% ของ ฿38,000', amount: 1900, method: 'หักจาก Settlement', status: 'รอนำส่ง/หักยอด' },
      { id: '2', feeType: 'ค่านายหน้าปล่อยเช่า (Leasing Commission)', unit: 'Ashton Asoke #2208', payer: 'เจ้าของ (คุณภัทรินทร์)', period: 'สัญญาใหม่ 1 ปี', calcBasis: 'ค่าเช่า 1 เดือน', amount: 45000, method: 'หักจาก Settlement', status: 'รับเงินแล้ว' },
      { id: '3', feeType: 'ค่าบริหารรายเดือน (Monthly Management)', unit: 'Ideo Q #0915', payer: 'เจ้าของ (Mrs. Sarah)', period: 'ต.ค. 2026', calcBasis: 'Fixed ฿2,000', amount: 2000, method: 'ออก Invoice แยก', status: 'รอนำส่ง/หักยอด' }
    ]);
  }
}
