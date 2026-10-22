import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AgencyFeeItem, AgencyFeesService, CreateAgencyFeePayload } from './agency-fees.service';
import { UnitsService, UnitItem } from '../units/units.service';

@Component({
  selector: 'app-agency-fees',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './agency-fees.component.html',
  styleUrl: './agency-fees.component.css'
})
export class AgencyFeesComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private agencyFeesService = inject(AgencyFeesService);
  private unitsService = inject(UnitsService);

  fees: AgencyFeeItem[] = [];
  units: UnitItem[] = [];
  isLoading = false;

  // Filters
  searchTerm = '';
  selectedFeeType: number | null = null;
  selectedStatus: number | null = null;
  selectedMethod: number | null = null;

  // KPIs
  totalAmount = 0;
  collectedAmount = 0;
  pendingDeductionAmount = 0;
  invoicedAmount = 0;

  // Create Modal State
  showCreateModal = false;
  isSubmitting = false;
  modalErrorMessage = '';

  newFee: CreateAgencyFeePayload = {
    unitId: '',
    feeType: 1, // MonthlyManagement
    description: '',
    period: 'ต.ค. 2026',
    calculationBasis: '5% ของค่าเช่า',
    amount: 0,
    collectionMethod: 1, // DeductFromSettlement
    status: 1, // PendingDeduction
    invoiceNo: '',
    note: ''
  };

  // Detail Modal State
  selectedFee: AgencyFeeItem | null = null;
  showDetailModal = false;

  ngOnInit(): void {
    const initial = this.route.snapshot.data['data'] as AgencyFeeItem[];
    if (initial && initial.length > 0) {
      this.fees = initial;
      this.calculateKpis();
    } else {
      this.loadFees();
    }

    this.loadUnits();
  }

  loadFees(): void {
    this.isLoading = true;
    this.agencyFeesService.getAgencyFeesData(
      this.searchTerm || undefined,
      this.selectedFeeType !== null ? this.selectedFeeType : undefined,
      this.selectedStatus !== null ? this.selectedStatus : undefined,
      this.selectedMethod !== null ? this.selectedMethod : undefined
    ).subscribe({
      next: (res) => {
        this.fees = res.items;
        this.totalAmount = res.totalAmount;
        this.collectedAmount = res.collectedAmount;
        this.pendingDeductionAmount = res.pendingDeductionAmount;
        this.invoicedAmount = res.invoicedAmount;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  loadUnits(): void {
    this.unitsService.getUnits().subscribe({
      next: (units) => {
        this.units = units;
        if (units.length > 0 && !this.newFee.unitId) {
          this.newFee.unitId = units[0].id;
          this.onUnitChange();
        }
      }
    });
  }

  calculateKpis(): void {
    this.totalAmount = this.fees.reduce((s, f) => s + f.amount, 0);
    this.collectedAmount = this.fees
      .filter(f => f.statusId === 3)
      .reduce((s, f) => s + f.amount, 0);
    this.pendingDeductionAmount = this.fees
      .filter(f => f.statusId === 1)
      .reduce((s, f) => s + f.amount, 0);
    this.invoicedAmount = this.fees
      .filter(f => f.statusId === 2)
      .reduce((s, f) => s + f.amount, 0);
  }

  onFilterChange(): void {
    this.loadFees();
  }

  onUnitChange(): void {
    const u = this.units.find(x => x.id === this.newFee.unitId);
    if (u) {
      if (this.newFee.feeType === 1) {
        // 5% of target rent
        const rent = u.targetRentPrice || 20000;
        this.newFee.amount = Math.round(rent * 0.05);
        this.newFee.calculationBasis = `5% ของค่าเช่า ฿${rent.toLocaleString()}`;
        this.newFee.description = `ค่าบริหารจัดการห้องชุดประจำเดือน`;
      } else if (this.newFee.feeType === 2) {
        // 1 month commission
        const rent = u.targetRentPrice || 20000;
        this.newFee.amount = rent;
        this.newFee.calculationBasis = `ค่าเช่า 1 เดือน (฿${rent.toLocaleString()})`;
        this.newFee.description = `ค่านายหน้าจัดหาผู้เช่าสัญญาใหม่ 1 ปี`;
      }
    }
  }

  openCreateModal(): void {
    this.newFee = {
      unitId: this.units.length > 0 ? this.units[0].id : '',
      feeType: 1,
      description: 'ค่าบริการบริหารจัดการห้องเช่ารายเดือน',
      period: 'ต.ค. 2026',
      calculationBasis: '5% ของค่าเช่า',
      amount: 0,
      collectionMethod: 1,
      status: 1,
      invoiceNo: '',
      note: ''
    };
    this.onUnitChange();
    this.modalErrorMessage = '';
    this.showCreateModal = true;
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
  }

  saveFee(): void {
    if (!this.newFee.unitId) {
      this.modalErrorMessage = 'กรุณาเลือกห้องชุด';
      return;
    }
    if (!this.newFee.description || !this.newFee.description.trim()) {
      this.modalErrorMessage = 'กรุณาระบุรายละเอียดค่าบริการ';
      return;
    }
    if (this.newFee.amount <= 0) {
      this.modalErrorMessage = 'จำนวนเงินต้องมากกว่า 0 บาท';
      return;
    }

    this.isSubmitting = true;
    this.modalErrorMessage = '';

    this.agencyFeesService.createAgencyFee(this.newFee).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.closeCreateModal();
        this.loadFees();
      },
      error: (err) => {
        this.isSubmitting = false;
        this.modalErrorMessage = err?.error?.error || 'เกิดข้อผิดพลาดในการบันทึกค่าบริการ';
      }
    });
  }

  openDetail(f: AgencyFeeItem): void {
    this.selectedFee = f;
    this.showDetailModal = true;
  }

  closeDetail(): void {
    this.showDetailModal = false;
    this.selectedFee = null;
  }

  markAsCollected(f: AgencyFeeItem): void {
    if (!confirm(`ต้องการปรับสถานะเป็น "รับเงินแล้ว" สำหรับรายการ ฿${f.amount.toLocaleString()} หรือไม่?`)) {
      return;
    }

    this.agencyFeesService.updateFeeStatus(f.id, 3).subscribe({
      next: () => {
        this.loadFees();
      },
      error: () => {
        alert('เกิดข้อผิดพลาดในการปรับสถานะ');
      }
    });
  }

  deleteFee(f: AgencyFeeItem): void {
    if (!confirm(`คุณต้องการลบรายการค่าบริการ "${f.feeType}" (฿${f.amount.toLocaleString()}) หรือไม่?`)) {
      return;
    }

    this.agencyFeesService.deleteAgencyFee(f.id).subscribe({
      next: () => {
        this.loadFees();
      },
      error: () => {
        alert('เกิดข้อผิดพลาดในการลบรายการ');
      }
    });
  }
}
