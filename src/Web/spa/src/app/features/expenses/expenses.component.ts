import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ExpenseItem, ExpensesService, CreateExpensePayload } from './expenses.service';
import { UnitsService, UnitItem } from '../units/units.service';

@Component({
  selector: 'app-expenses',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './expenses.component.html',
  styleUrl: './expenses.component.css'
})
export class ExpensesComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private expensesService = inject(ExpensesService);
  private unitsService = inject(UnitsService);

  expenses: ExpenseItem[] = [];
  units: UnitItem[] = [];
  isLoading = false;

  // Filters
  searchTerm = '';
  selectedCategory: number | null = null;
  selectedDeductible: boolean | null = null;

  // KPI Metrics
  totalAmount = 0;
  deductibleAmount = 0;
  agencyAdvancedAmount = 0;

  // Modal State
  showModal = false;
  isSubmitting = false;
  modalErrorMessage = '';

  newExpense: CreateExpensePayload = {
    unitId: '',
    category: 1,
    description: '',
    amount: 0,
    incurredDate: new Date().toISOString().substring(0, 10),
    paidBy: 1,
    isDeductibleFromOwner: true,
    receiptUrl: '',
    note: ''
  };

  // Detail Modal
  selectedExpense: ExpenseItem | null = null;
  showDetailModal = false;

  ngOnInit(): void {
    const initial = this.route.snapshot.data['data'] as ExpenseItem[];
    if (initial && initial.length > 0) {
      this.expenses = initial;
      this.calculateKpis();
    } else {
      this.loadExpenses();
    }

    this.loadUnits();
  }

  loadExpenses(): void {
    this.isLoading = true;
    this.expensesService.getExpensesData(
      this.searchTerm || undefined,
      this.selectedCategory !== null ? this.selectedCategory : undefined,
      this.selectedDeductible !== null ? this.selectedDeductible : undefined
    ).subscribe({
      next: (res) => {
        this.expenses = res.items;
        this.totalAmount = res.totalAmount;
        this.deductibleAmount = res.deductibleAmount;
        this.agencyAdvancedAmount = this.expenses
          .filter(e => e.paidById === 1)
          .reduce((sum, e) => sum + e.amount, 0);
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
        if (units.length > 0 && !this.newExpense.unitId) {
          this.newExpense.unitId = units[0].id;
        }
      }
    });
  }

  calculateKpis(): void {
    this.totalAmount = this.expenses.reduce((s, e) => s + e.amount, 0);
    this.deductibleAmount = this.expenses
      .filter(e => e.deductible)
      .reduce((s, e) => s + e.amount, 0);
    this.agencyAdvancedAmount = this.expenses
      .filter(e => e.paidById === 1)
      .reduce((s, e) => s + e.amount, 0);
  }

  onFilterChange(): void {
    this.loadExpenses();
  }

  openCreateModal(): void {
    this.newExpense = {
      unitId: this.units.length > 0 ? this.units[0].id : '',
      category: 1,
      description: '',
      amount: 0,
      incurredDate: new Date().toISOString().substring(0, 10),
      paidBy: 1,
      isDeductibleFromOwner: true,
      receiptUrl: '',
      note: ''
    };
    this.modalErrorMessage = '';
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
  }

  saveExpense(): void {
    if (!this.newExpense.unitId) {
      this.modalErrorMessage = 'กรุณาเลือกห้องชุด';
      return;
    }
    if (!this.newExpense.description || !this.newExpense.description.trim()) {
      this.modalErrorMessage = 'กรุณาระบุรายละเอียดค่าใช้จ่าย';
      return;
    }
    if (this.newExpense.amount <= 0) {
      this.modalErrorMessage = 'จำนวนเงินต้องมากกว่า 0 บาท';
      return;
    }

    this.isSubmitting = true;
    this.modalErrorMessage = '';

    this.expensesService.createExpense(this.newExpense).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.closeModal();
        this.loadExpenses();
      },
      error: (err) => {
        this.isSubmitting = false;
        this.modalErrorMessage = err?.error?.error || 'เกิดข้อผิดพลาดในการบันทึกค่าใช้จ่าย';
      }
    });
  }

  openDetail(e: ExpenseItem): void {
    this.selectedExpense = e;
    this.showDetailModal = true;
  }

  closeDetail(): void {
    this.showDetailModal = false;
    this.selectedExpense = null;
  }

  deleteExpense(e: ExpenseItem): void {
    if (!confirm(`คุณต้องการลบรายการ "${e.description}" (฿ ${e.amount.toLocaleString()}) หรือไม่?`)) {
      return;
    }

    this.expensesService.deleteExpense(e.id).subscribe({
      next: () => {
        this.loadExpenses();
      },
      error: () => {
        alert('เกิดข้อผิดพลาดในการลบรายการ');
      }
    });
  }
}
