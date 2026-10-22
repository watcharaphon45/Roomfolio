import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { LeasesService, LeaseItem, CreateLeasePayload, UpdateLeasePayload } from './leases.service';
import { UnitsService, UnitItem } from '../units/units.service';
import { TenantsService, TenantItem } from '../tenants/tenants.service';

@Component({
  selector: 'app-leases',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './leases.component.html',
  styleUrl: './leases.component.css'
})
export class LeasesComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private leasesService = inject(LeasesService);
  private unitsService = inject(UnitsService);
  private tenantsService = inject(TenantsService);

  leases: LeaseItem[] = [];
  units: UnitItem[] = [];
  tenants: TenantItem[] = [];

  searchTerm = '';
  selectedPaymentMode: number | null = null;
  selectedStatus: number | null = null;
  isLoading = false;
  isSaving = false;

  isModalOpen = false;
  isEditMode = false;
  editingLeaseId: string | null = null;

  currentLease: CreateLeasePayload & {
    terminatedAt?: string;
    terminationReason?: string;
  } = {
    unitId: '',
    tenantId: '',
    leaseNumber: '',
    startDate: '',
    endDate: '',
    rentDueDay: 1,
    monthlyRent: 0,
    paymentMode: 1, // 1 = DirectToOwner, 2 = ThroughAgent
    depositAmount: 0,
    depositHolder: 1, // 1 = Owner, 2 = Agency
    status: 2, // 2 = Active
    signedAt: '',
    note: ''
  };

  ngOnInit(): void {
    this.leases = this.route.snapshot.data['data'] as LeaseItem[] || [];
    this.loadLeases();
    this.loadDropdownData();
  }

  loadLeases(): void {
    this.isLoading = true;
    this.leasesService.getLeases(
      this.searchTerm,
      this.selectedPaymentMode || undefined,
      this.selectedStatus || undefined
    ).subscribe({
      next: (res) => {
        this.leases = res;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  loadDropdownData(): void {
    this.unitsService.getUnits().subscribe(u => this.units = u);
    this.tenantsService.getTenants().subscribe(t => this.tenants = t);
  }

  onSearch(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchTerm = input.value;
    this.loadLeases();
  }

  setPaymentModeFilter(mode: number | null): void {
    this.selectedPaymentMode = mode;
    this.loadLeases();
  }

  get directToOwnerCount(): number {
    return this.leases.filter(l => l.paymentMode === 1).length;
  }

  get throughAgentCount(): number {
    return this.leases.filter(l => l.paymentMode === 2).length;
  }

  get totalMonthlyRent(): number {
    return this.leases
      .filter(l => l.status === 2 || l.status === 3)
      .reduce((sum, l) => sum + l.monthlyRent, 0);
  }

  openCreateModal(): void {
    this.isEditMode = false;
    this.editingLeaseId = null;

    // Pick first available unit if exists
    const availableUnit = this.units.find(u => u.status === 1) || this.units[0];
    const defaultTenant = this.tenants[0];

    const today = new Date();
    const oneYearLater = new Date();
    oneYearLater.setFullYear(today.getFullYear() + 1);

    const pad = (n: number) => n.toString().padStart(2, '0');
    const startStr = `${today.getFullYear()}-${pad(today.getMonth() + 1)}-01`;
    const endStr = `${oneYearLater.getFullYear()}-${pad(oneYearLater.getMonth() + 1)}-01`;

    const unitPrice = availableUnit?.targetRentPrice || 25000;

    this.currentLease = {
      unitId: availableUnit?.id || '',
      tenantId: defaultTenant?.id || '',
      leaseNumber: `LS-${today.getFullYear()}-${pad(Math.floor(Math.random() * 900) + 100)}`,
      startDate: startStr,
      endDate: endStr,
      rentDueDay: 1,
      monthlyRent: unitPrice,
      paymentMode: availableUnit?.paymentMode || 2,
      depositAmount: unitPrice * 2,
      depositHolder: availableUnit?.paymentMode === 2 ? 2 : 1,
      status: 2, // Active
      signedAt: `${today.getFullYear()}-${pad(today.getMonth() + 1)}-${pad(today.getDate())}`,
      note: ''
    };

    this.isModalOpen = true;
  }

  onUnitChanged(): void {
    const selected = this.units.find(u => u.id === this.currentLease.unitId);
    if (selected) {
      this.currentLease.monthlyRent = selected.targetRentPrice;
      this.currentLease.depositAmount = selected.targetRentPrice * 2;
      this.currentLease.paymentMode = selected.paymentMode;
      this.currentLease.depositHolder = selected.paymentMode === 2 ? 2 : 1;
    }
  }

  openEditModal(lease: LeaseItem): void {
    this.isEditMode = true;
    this.editingLeaseId = lease.id;
    this.currentLease = {
      unitId: lease.unitId,
      tenantId: lease.tenantId,
      leaseNumber: lease.leaseNumber,
      startDate: lease.startDate,
      endDate: lease.endDate,
      rentDueDay: lease.rentDueDay,
      monthlyRent: lease.monthlyRent,
      paymentMode: lease.paymentMode,
      depositAmount: lease.depositAmount,
      depositHolder: lease.depositHolder,
      status: lease.status,
      signedAt: lease.signedAt || '',
      note: lease.note || ''
    };
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
  }

  saveLease(): void {
    if (!this.currentLease.leaseNumber) {
      alert('กรุณากรอกเลขที่สัญญา');
      return;
    }
    if (!this.currentLease.unitId) {
      alert('กรุณาเลือกห้องชุด');
      return;
    }
    if (!this.currentLease.tenantId) {
      alert('กรุณาเลือกผู้เช่า');
      return;
    }
    if (this.currentLease.monthlyRent <= 0) {
      alert('กรุณาระบุค่าเช่าต่อเดือนที่มากกว่า 0');
      return;
    }

    this.isSaving = true;

    if (this.isEditMode && this.editingLeaseId) {
      const payload: UpdateLeasePayload = {
        unitId: this.currentLease.unitId,
        tenantId: this.currentLease.tenantId,
        leaseNumber: this.currentLease.leaseNumber,
        startDate: this.currentLease.startDate,
        endDate: this.currentLease.endDate,
        rentDueDay: Number(this.currentLease.rentDueDay),
        monthlyRent: Number(this.currentLease.monthlyRent),
        paymentMode: Number(this.currentLease.paymentMode),
        depositAmount: Number(this.currentLease.depositAmount),
        depositHolder: Number(this.currentLease.depositHolder),
        status: Number(this.currentLease.status),
        signedAt: this.currentLease.signedAt || undefined,
        terminatedAt: this.currentLease.terminatedAt || undefined,
        terminationReason: this.currentLease.terminationReason || undefined,
        note: this.currentLease.note || undefined
      };

      this.leasesService.updateLease(this.editingLeaseId, payload).subscribe({
        next: () => {
          this.isSaving = false;
          this.closeModal();
          this.loadLeases();
        },
        error: (err) => {
          console.error('Failed to update lease:', err);
          alert('ไม่สามารถอัปเดตสัญญาได้: ' + (err.error?.message || err.message));
          this.isSaving = false;
        }
      });
    } else {
      const payload: CreateLeasePayload = {
        unitId: this.currentLease.unitId,
        tenantId: this.currentLease.tenantId,
        leaseNumber: this.currentLease.leaseNumber,
        startDate: this.currentLease.startDate,
        endDate: this.currentLease.endDate,
        rentDueDay: Number(this.currentLease.rentDueDay),
        monthlyRent: Number(this.currentLease.monthlyRent),
        paymentMode: Number(this.currentLease.paymentMode),
        depositAmount: Number(this.currentLease.depositAmount),
        depositHolder: Number(this.currentLease.depositHolder),
        status: Number(this.currentLease.status),
        signedAt: this.currentLease.signedAt || undefined,
        note: this.currentLease.note || undefined
      };

      this.leasesService.createLease(payload).subscribe({
        next: () => {
          this.isSaving = false;
          this.closeModal();
          this.loadLeases();
        },
        error: (err) => {
          console.error('Failed to create lease:', err);
          alert('ไม่สามารถทำสัญญาใหม่ได้: ' + (err.error?.message || err.message));
          this.isSaving = false;
        }
      });
    }
  }

  deleteLease(id: string, leaseNo: string): void {
    if (!confirm(`คุณต้องการลบสัญญา "${leaseNo}" ใช่หรือไม่? ระบบจะปรับสถานะห้องเป็นว่างโดยอัตโนมัติหากไม่มีสัญญาอื่น`)) {
      return;
    }

    this.leasesService.deleteLease(id).subscribe({
      next: () => {
        this.loadLeases();
      },
      error: (err) => {
        console.error('Failed to delete lease:', err);
        alert('ไม่สามารถลบสัญญาได้: ' + (err.error?.message || err.message));
      }
    });
  }
}
