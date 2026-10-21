import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

export interface ReportItem {
  id: string;
  title: string;
  desc: string;
  format: string;
  icon: string;
}

@Injectable({
  providedIn: 'root'
})
export class ReportsService {
  getReports(): Observable<ReportItem[]> {
    return of([
      {
        id: '1',
        title: 'Owner Statement (PDF/CSV)',
        desc: 'สรุปรายรับค่าเช่า ค่าใช้จ่ายหักได้ ค่าบริหาร และเงินสุทธิโอนเจ้าของห้องแต่ละราย',
        format: 'PDF / CSV',
        icon: '<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path><polyline points="14 2 14 8 20 8"></polyline></svg>'
      },
      {
        id: '2',
        title: 'Agency Cash Held Report',
        desc: 'รายงานตรวจสอบเงินที่นายหน้ารับแทนและยังถือครองอยู่ โดยไม่นับรวมยอดจ่ายตรง',
        format: 'XLSX / PDF',
        icon: '<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="1" y="4" width="22" height="16" rx="2" ry="2"></rect><line x1="1" y1="10" x2="23" y2="10"></line></svg>'
      },
      {
        id: '3',
        title: 'Rent Due & Outstanding',
        desc: 'รายงานครบกำหนดชำระค่าเช่า ประวัติการรับเงินจริง และยอดค้างชำระแบ่งตามระยะเวลา',
        format: 'XLSX',
        icon: '<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="12" y1="1" x2="12" y2="23"></line><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"></path></svg>'
      },
      {
        id: '4',
        title: 'Lease Expiry 30/60/90 Days',
        desc: 'รายงานคาดการณ์สัญญาหมดอายุ เพื่อเตือนต่อสัญญาหรือเตรียมปล่อยเช่าใหม่ล่วงหน้า',
        format: 'XLSX / PDF',
        icon: '<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"></circle><polyline points="12 6 12 12 16 14"></polyline></svg>'
      }
    ]);
  }
}
