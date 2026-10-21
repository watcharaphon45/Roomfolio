import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

export interface TenantItem {
  id: string;
  name: string;
  phone: string;
  email: string;
  unit: string;
  leaseNo: string;
  deposit: number;
}

@Injectable({
  providedIn: 'root'
})
export class TenantsService {
  getTenants(): Observable<TenantItem[]> {
    return of([
      { id: '1', name: 'คุณกิตติศักดิ์ พ.', phone: '081-998-1122', email: 'kittisak.p@gmail.com', unit: 'Noble Ploenchit #1402', leaseNo: 'LS-2025-081', deposit: 76000 },
      { id: '2', name: 'Mr. David Miller', phone: '+66 84 123 4567', email: 'david.m@corporate.com', unit: 'Ashton Asoke #2208', leaseNo: 'LS-2026-012', deposit: 90000 },
      { id: '3', name: 'คุณนพวรรณ ช.', phone: '086-554-3321', email: 'nopawan.c@yahoo.com', unit: 'Ideo Q #0915', leaseNo: 'LS-2026-034', deposit: 44000 }
    ]);
  }
}
