import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import {
  RentCollectionService,
  RentCollectionSummary,
  RentChargeItem,
  RecordPaymentPayload
} from './rent-collection.service';

@Component({
  selector: 'app-rent-collection',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './rent-collection.component.html',
  styleUrl: './rent-collection.component.css'
})
export class RentCollectionComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private rentCollectionService = inject(RentCollectionService);

  summary: RentCollectionSummary | null = null;
  searchTerm = '';
  selectedPaymentMode: number | null = null;
  isLoading = false;
  isSaving = false;

  // Payment Recording Modal
  isPaymentModalOpen = false;
  selectedCharge: RentChargeItem | null = null;

  paymentForm: RecordPaymentPayload = {
    rentChargeId: '',
    amount: 0,
    paymentDate: '',
    method: 1, // BankTransfer
    paymentMode: 2, // ThroughAgent
    receivedByParty: 3, // Agency
    referenceNo: '',
    note: ''
  };

  ngOnInit(): void {
    this.summary = this.route.snapshot.data['data'] as RentCollectionSummary;
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    this.rentCollectionService
      .getRentCollectionData(this.searchTerm, this.selectedPaymentMode || undefined)
      .subscribe({
        next: (res) => {
          this.summary = res;
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
        }
      });
  }

  onSearch(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchTerm = input.value;
    this.loadData();
  }

  setPaymentModeFilter(mode: number | null): void {
    this.selectedPaymentMode = mode;
    this.loadData();
  }

  get directToOwnerCount(): number {
    return this.summary?.items.filter(i => i.paymentMode === 1).length || 0;
  }

  get throughAgentCount(): number {
    return this.summary?.items.filter(i => i.paymentMode === 2).length || 0;
  }

  openPaymentModal(item?: RentChargeItem): void {
    const today = new Date();
    const pad = (n: number) => n.toString().padStart(2, '0');
    const todayStr = `${today.getFullYear()}-${pad(today.getMonth() + 1)}-${pad(today.getDate())}`;

    if (item) {
      this.selectedCharge = item;
      this.paymentForm = {
        rentChargeId: item.id,
        amount: item.outstanding > 0 ? item.outstanding : item.chargeAmount,
        paymentDate: todayStr,
        method: 1,
        paymentMode: item.paymentMode,
        receivedByParty: item.paymentMode === 2 ? 3 : 2, // 3 = Agency, 2 = Owner
        referenceNo: '',
        note: ''
      };
    } else {
      // Pick first outstanding charge or first charge
      const target = this.summary?.items.find(i => i.outstanding > 0) || this.summary?.items[0];
      this.selectedCharge = target || null;

      this.paymentForm = {
        rentChargeId: target?.id || '',
        amount: target ? (target.outstanding > 0 ? target.outstanding : target.chargeAmount) : 0,
        paymentDate: todayStr,
        method: 1,
        paymentMode: target?.paymentMode || 2,
        receivedByParty: target?.paymentMode === 2 ? 3 : 2,
        referenceNo: '',
        note: ''
      };
    }

    this.isPaymentModalOpen = true;
  }

  onChargeSelectionChanged(): void {
    const found = this.summary?.items.find(i => i.id === this.paymentForm.rentChargeId);
    if (found) {
      this.selectedCharge = found;
      this.paymentForm.amount = found.outstanding > 0 ? found.outstanding : found.chargeAmount;
      this.paymentForm.paymentMode = found.paymentMode;
      this.paymentForm.receivedByParty = found.paymentMode === 2 ? 3 : 2;
    }
  }

  closePaymentModal(): void {
    this.isPaymentModalOpen = false;
    this.selectedCharge = null;
  }

  submitPayment(): void {
    if (!this.paymentForm.rentChargeId) {
      alert('กรุณาเลือกงวดค่าเช่าที่ต้องการบันทึกการชำระ');
      return;
    }

    if (this.paymentForm.amount <= 0) {
      alert('กรุณาระบุจำนวนเงินที่รับชำระมากกว่า 0');
      return;
    }

    this.isSaving = true;

    this.rentCollectionService.recordPayment(this.paymentForm).subscribe({
      next: () => {
        this.isSaving = false;
        this.closePaymentModal();
        this.loadData();
      },
      error: (err) => {
        console.error('Failed to record payment:', err);
        alert('ไม่สามารถบันทึกการชำระเงินได้: ' + (err.error?.message || err.message));
        this.isSaving = false;
      }
    });
  }
}
