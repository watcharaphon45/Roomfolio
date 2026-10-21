import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

export interface ExpenseItem {
  id: string;
  unit: string;
  date: string;
  category: string;
  description: string;
  paidBy: string;
  amount: number;
  deductible: boolean;
  status: string;
}

@Injectable({
  providedIn: 'root'
})
export class ExpensesService {
  getExpenses(): Observable<ExpenseItem[]> {
    return of([
      { id: '1', unit: 'Noble Ploenchit #1402', date: '12 ต.ค. 2026', category: 'ทำความสะอาด', description: 'Deep cleaning ก่อนผู้เช่าเข้าอยู่', paidBy: 'Agency (สำรองจ่าย)', amount: 1500, deductible: true, status: 'ยืนยันแล้ว' },
      { id: '2', unit: 'Ashton Asoke #2208', date: '08 ต.ค. 2026', category: 'กุญแจ/คีย์การ์ด', description: 'ทำบัตรคอนโดเพิ่ม 1 ใบ', paidBy: 'Agency (สำรองจ่าย)', amount: 800, deductible: true, status: 'ยืนยันแล้ว' },
      { id: '3', unit: 'Ideo Q #0915', date: '02 ต.ค. 2026', category: 'ค่าส่วนกลาง', description: 'เจ้าของชำระตรงกับนิติ', paidBy: 'Owner (จ่ายเอง)', amount: 18500, deductible: false, status: 'บันทึกเพื่อรายงาน' }
    ]);
  }
}
