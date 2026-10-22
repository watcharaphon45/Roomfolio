import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { DepositItem, DepositsService, AddTransactionPayload } from './deposits.service';

@Component({
  selector: 'app-deposits',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './deposits.component.html',
  styleUrl: './deposits.component.css'
})
export class DepositsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private depositsService = inject(DepositsService);

  deposits: DepositItem[] = [];
  isLoading = false;

  // Filters
  searchTerm = '';
  selectedHolder: number | null = null;
  selectedStatus: number | null = null;

  // KPIs
  totalInitialAmount = 0;
  agencyHeldAmount = 0;
  ownerHeldAmount = 0;
  totalDeductedAmount = 0;
  totalBalance = 0;

  // Transaction Modal State
  selectedDeposit: DepositItem | null = null;
  showTransactionModal = false;
  isSubmittingTx = false;
  txErrorMessage = '';

  newTransaction: AddTransactionPayload = {
    type: 2, // 2 = Deduction, 3 = Refund
    amount: 0,
    transactionDate: new Date().toISOString().substring(0, 10),
    description: '',
    referenceNo: '',
    note: ''
  };

  ngOnInit(): void {
    const initial = this.route.snapshot.data['data'] as DepositItem[];
    if (initial && initial.length > 0) {
      this.deposits = initial;
      this.calculateKpis();
    } else {
      this.loadDeposits();
    }
  }

  loadDeposits(): void {
    this.isLoading = true;
    this.depositsService.getDepositsData(
      this.searchTerm || undefined,
      this.selectedHolder !== null ? this.selectedHolder : undefined,
      this.selectedStatus !== null ? this.selectedStatus : undefined
    ).subscribe({
      next: (res) => {
        this.deposits = res.items;
        this.totalInitialAmount = res.totalInitialAmount;
        this.agencyHeldAmount = res.agencyHeldAmount;
        this.ownerHeldAmount = res.ownerHeldAmount;
        this.totalDeductedAmount = res.totalDeductedAmount;
        this.totalBalance = res.totalBalance;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  calculateKpis(): void {
    this.totalInitialAmount = this.deposits.reduce((s, d) => s + d.initialAmount, 0);
    this.agencyHeldAmount = this.deposits
      .filter(d => d.holderId === 2)
      .reduce((s, d) => s + d.balance, 0);
    this.ownerHeldAmount = this.deposits
      .filter(d => d.holderId === 1)
      .reduce((s, d) => s + d.balance, 0);
    this.totalDeductedAmount = this.deposits.reduce((s, d) => s + d.deducted, 0);
    this.totalBalance = this.deposits.reduce((s, d) => s + d.balance, 0);
  }

  onFilterChange(): void {
    this.loadDeposits();
  }

  openTransactionModal(deposit: DepositItem): void {
    this.selectedDeposit = deposit;
    this.newTransaction = {
      type: 2,
      amount: 0,
      transactionDate: new Date().toISOString().substring(0, 10),
      description: '',
      referenceNo: '',
      note: ''
    };
    this.txErrorMessage = '';
    this.showTransactionModal = true;
  }

  closeTransactionModal(): void {
    this.showTransactionModal = false;
    this.selectedDeposit = null;
  }

  saveTransaction(): void {
    if (!this.selectedDeposit) return;

    if (this.newTransaction.amount <= 0) {
      this.txErrorMessage = 'กรุณาระบุจำนวนเงินที่มากกว่า 0 บาท';
      return;
    }

    if (this.newTransaction.amount > this.selectedDeposit.balance) {
      this.txErrorMessage = `ยอดทำรายการ (฿${this.newTransaction.amount.toLocaleString()}) เกินยอดเงินประกันคงเหลือ (฿${this.selectedDeposit.balance.toLocaleString()})`;
      return;
    }

    if (!this.newTransaction.description || !this.newTransaction.description.trim()) {
      this.txErrorMessage = 'กรุณาระบุรายละเอียดการทำรายการ';
      return;
    }

    this.isSubmittingTx = true;
    this.txErrorMessage = '';

    this.depositsService.addTransaction(this.selectedDeposit.id, this.newTransaction).subscribe({
      next: () => {
        this.isSubmittingTx = false;
        this.closeTransactionModal();
        this.loadDeposits();
      },
      error: (err) => {
        this.isSubmittingTx = false;
        this.txErrorMessage = err?.error?.error || 'เกิดข้อผิดพลาดในการบันทึกธุรกรรมเงินประกัน';
      }
    });
  }

  deleteDeposit(d: DepositItem): void {
    if (!confirm(`คุณต้องการลบข้อมูลเงินประกันของสัญญา ${d.leaseNo} (${d.unit}) หรือไม่?`)) {
      return;
    }

    this.depositsService.deleteDeposit(d.id).subscribe({
      next: () => {
        this.loadDeposits();
      },
      error: () => {
        alert('เกิดข้อผิดพลาดในการลบรายการ');
      }
    });
  }
}
