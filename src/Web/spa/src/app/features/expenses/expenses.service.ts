import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, map, of } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ExpenseItem {
  id: string;
  unitId: string;
  unit: string;
  date: string;
  rawDate: string;
  categoryId: number;
  category: string;
  description: string;
  paidById: number;
  paidBy: string;
  amount: number;
  deductible: boolean;
  status: string;
  receiptUrl?: string;
  note?: string;
}

export interface ExpensesResponse {
  totalCount: number;
  totalAmount: number;
  deductibleAmount: number;
  items: ExpenseItem[];
}

export interface CreateExpensePayload {
  unitId: string;
  category: number;
  description: string;
  amount: number;
  incurredDate?: string;
  paidBy: number;
  isDeductibleFromOwner: boolean;
  receiptUrl?: string;
  note?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ExpensesService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/expenses`;

  getExpensesData(search?: string, category?: number, deductible?: boolean): Observable<ExpensesResponse> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (category !== undefined && category !== null) params = params.set('category', category.toString());
    if (deductible !== undefined && deductible !== null) params = params.set('deductible', deductible.toString());

    return this.http.get<ExpensesResponse>(this.apiUrl, { params }).pipe(
      catchError(err => {
        console.warn('API getExpensesData error, falling back:', err);
        const fallbackItems: ExpenseItem[] = [
          {
            id: '1',
            unitId: 'u1',
            unit: 'Noble Ploenchit #1402',
            date: '12 ต.ค. 2026',
            rawDate: '2026-10-12',
            categoryId: 1,
            category: 'ทำความสะอาด',
            description: 'Deep cleaning ก่อนผู้เช่าเข้าอยู่',
            paidById: 1,
            paidBy: 'Agency (สำรองจ่าย)',
            amount: 1500,
            deductible: true,
            status: 'ยืนยันแล้ว',
            note: 'แนบใบเสร็จแม่บ้านทำความสะอาดเรียบร้อย'
          },
          {
            id: '2',
            unitId: 'u2',
            unit: 'Ashton Asoke #2208',
            date: '08 ต.ค. 2026',
            rawDate: '2026-10-08',
            categoryId: 3,
            category: 'กุญแจ/คีย์การ์ด',
            description: 'ทำบัตรคอนโดเพิ่ม 1 ใบ',
            paidById: 1,
            paidBy: 'Agency (สำรองจ่าย)',
            amount: 800,
            deductible: true,
            status: 'ยืนยันแล้ว',
            note: 'ออกใบเสร็จจากนิติบุคคลอาคารชุด'
          },
          {
            id: '3',
            unitId: 'u3',
            unit: 'Ideo Q Sukhumvit 36 #0915',
            date: '02 ต.ค. 2026',
            rawDate: '2026-10-02',
            categoryId: 5,
            category: 'ค่าส่วนกลาง',
            description: 'ค่าส่วนกลางประจำปี เจ้าของชำระตรงกับนิติ',
            paidById: 2,
            paidBy: 'Owner (จ่ายเอง)',
            amount: 18500,
            deductible: false,
            status: 'ยืนยันแล้ว',
            note: 'บันทึกเพื่อรายงานสถิติค่าใช้จ่ายประจำห้อง'
          }
        ];

        return of({
          totalCount: fallbackItems.length,
          totalAmount: 20800,
          deductibleAmount: 2300,
          items: fallbackItems
        });
      })
    );
  }

  getExpenses(): Observable<ExpenseItem[]> {
    return this.getExpensesData().pipe(map(res => res.items));
  }

  createExpense(payload: CreateExpensePayload): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.apiUrl, payload);
  }

  deleteExpense(id: string): Observable<{ success: boolean }> {
    return this.http.delete<{ success: boolean }>(`${this.apiUrl}/${id}`);
  }
}
