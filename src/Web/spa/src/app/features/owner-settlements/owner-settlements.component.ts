import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import {
  OwnerSettlementsService,
  SettlementPipelineSummary,
  OwnerSettlementItem,
  CreateSettlementPayload,
  UpdateSettlementStatusPayload
} from './owner-settlements.service';
import { OwnersService, OwnerItem } from '../owners/owners.service';

@Component({
  selector: 'app-owner-settlements',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './owner-settlements.component.html',
  styleUrl: './owner-settlements.component.css'
})
export class OwnerSettlementsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private settlementService = inject(OwnerSettlementsService);
  private ownersService = inject(OwnersService);

  data: SettlementPipelineSummary | null = null;
  owners: OwnerItem[] = [];

  searchTerm = '';
  selectedStatusFilter: number | null = null;
  isLoading = false;
  isSaving = false;

  // Generate Settlement Modal
  isGenerateModalOpen = false;
  newSettlement: CreateSettlementPayload = {
    ownerId: '',
    periodStart: '',
    periodEnd: '',
    grossRentCollected: 0,
    managementFeePercent: 5,
    expensesDeducted: 0,
    note: ''
  };

  // Disburse Payment Modal
  isDisburseModalOpen = false;
  targetSettlement: OwnerSettlementItem | null = null;
  disburseForm = {
    transferReference: '',
    note: ''
  };

  // View Statement Lines Modal
  isDetailsModalOpen = false;
  viewingSettlement: OwnerSettlementItem | null = null;

  ngOnInit(): void {
    this.data = this.route.snapshot.data['data'] as SettlementPipelineSummary;
    this.loadData();
    this.loadOwners();
  }

  loadData(): void {
    this.isLoading = true;
    this.settlementService
      .getSettlementsData(this.searchTerm, this.selectedStatusFilter || undefined)
      .subscribe({
        next: (res) => {
          this.data = res;
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
        }
      });
  }

  loadOwners(): void {
    this.ownersService.getOwners().subscribe(o => this.owners = o);
  }

  onSearch(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchTerm = input.value;
    this.loadData();
  }

  filterByStatus(status: number | null): void {
    this.selectedStatusFilter = this.selectedStatusFilter === status ? null : status;
    this.loadData();
  }

  get calculatedNet(): number {
    const fee = (this.newSettlement.grossRentCollected * this.newSettlement.managementFeePercent) / 100;
    return this.newSettlement.grossRentCollected - this.newSettlement.expensesDeducted - fee;
  }

  openGenerateModal(): void {
    const today = new Date();
    const pad = (n: number) => n.toString().padStart(2, '0');
    const startStr = `${today.getFullYear()}-${pad(today.getMonth() + 1)}-01`;

    const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0).getDate();
    const endStr = `${today.getFullYear()}-${pad(today.getMonth() + 1)}-${pad(lastDay)}`;

    this.newSettlement = {
      ownerId: this.owners[0]?.id || '',
      periodStart: startStr,
      periodEnd: endStr,
      grossRentCollected: 45000,
      managementFeePercent: 5,
      expensesDeducted: 0,
      note: ''
    };

    this.isGenerateModalOpen = true;
  }

  closeGenerateModal(): void {
    this.isGenerateModalOpen = false;
  }

  submitGenerateSettlement(): void {
    if (!this.newSettlement.ownerId) {
      alert('กรุณาเลือกเจ้าของห้อง');
      return;
    }

    if (this.newSettlement.grossRentCollected < 0) {
      alert('ยอดค่าเช่ารับจริงต้องไม่ติดลบ');
      return;
    }

    this.isSaving = true;
    this.settlementService.createSettlement(this.newSettlement).subscribe({
      next: () => {
        this.isSaving = false;
        this.closeGenerateModal();
        this.loadData();
      },
      error: (err) => {
        console.error('Failed to create settlement:', err);
        alert('ไม่สามารถสร้างรอบสรุปยอดได้: ' + (err.error?.message || err.message));
        this.isSaving = false;
      }
    });
  }

  openDisburseModal(s: OwnerSettlementItem): void {
    this.targetSettlement = s;
    this.disburseForm = {
      transferReference: `KBANK-SETTLE-${new Date().getTime().toString().slice(-6)}`,
      note: `โอนเงินนำส่งเข้าบัญชี ${s.bankName || ''} เลขที่ ${s.bankAccountNumber || ''}`
    };
    this.isDisburseModalOpen = true;
  }

  closeDisburseModal(): void {
    this.isDisburseModalOpen = false;
    this.targetSettlement = null;
  }

  submitDisbursePayment(): void {
    if (!this.targetSettlement) return;
    if (!this.disburseForm.transferReference) {
      alert('กรุณากรอกเลขที่อ้างอิงสลิปโอนเงิน');
      return;
    }

    this.isSaving = true;
    const payload: UpdateSettlementStatusPayload = {
      status: 4, // Paid
      transferReference: this.disburseForm.transferReference,
      note: this.disburseForm.note
    };

    this.settlementService.updateSettlementStatus(this.targetSettlement.id, payload).subscribe({
      next: () => {
        this.isSaving = false;
        this.closeDisburseModal();
        this.loadData();
      },
      error: (err) => {
        console.error('Failed to disburse payment:', err);
        alert('ไม่สามารถบันทึกการโอนเงินได้: ' + (err.error?.message || err.message));
        this.isSaving = false;
      }
    });
  }

  openDetailsModal(s: OwnerSettlementItem): void {
    this.viewingSettlement = s;
    this.isDetailsModalOpen = true;
  }

  closeDetailsModal(): void {
    this.isDetailsModalOpen = false;
    this.viewingSettlement = null;
  }

  advanceStatus(s: OwnerSettlementItem, nextStatus: number): void {
    const statusNames: Record<number, string> = {
      2: 'ส่งตรวจทาน (Review)',
      3: 'อนุมัติการโอน (Approved)',
      4: 'โอนแล้วเสร็จ (Paid)'
    };

    if (!confirm(`คุณต้องการเปลี่ยนสถานะรอบโอนเงินของ "${s.ownerName}" เป็น ${statusNames[nextStatus]} ใช่หรือไม่?`)) {
      return;
    }

    const payload: UpdateSettlementStatusPayload = {
      status: nextStatus
    };

    this.settlementService.updateSettlementStatus(s.id, payload).subscribe({
      next: () => {
        this.loadData();
      },
      error: (err) => {
        console.error('Failed to advance status:', err);
        alert('ไม่สามารถเปลี่ยนสถานะได้: ' + (err.error?.message || err.message));
      }
    });
  }
}
