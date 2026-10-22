import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { TenantsService, TenantItem, CreateTenantPayload, UpdateTenantPayload } from './tenants.service';

@Component({
  selector: 'app-tenants',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tenants.component.html',
  styleUrl: './tenants.component.css'
})
export class TenantsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private tenantsService = inject(TenantsService);

  tenants: TenantItem[] = [];
  searchTerm = '';
  isLoading = false;
  isModalOpen = false;
  isEditMode = false;
  editingTenantId: string | null = null;
  isSaving = false;

  currentTenant: CreateTenantPayload & { status?: string } = {
    fullName: '',
    phone: '',
    email: '',
    idCardOrPassport: '',
    lineId: '',
    emergencyContact: '',
    note: '',
    status: 'Active'
  };

  ngOnInit(): void {
    this.tenants = this.route.snapshot.data['data'] as TenantItem[] || [];
    this.loadTenants();
  }

  loadTenants(): void {
    this.isLoading = true;
    this.tenantsService.getTenants(this.searchTerm).subscribe({
      next: (res) => {
        this.tenants = res;
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
    this.loadTenants();
  }

  get totalDeposits(): number {
    return this.tenants.reduce((sum, t) => sum + (t.currentDeposit || 0), 0);
  }

  get activeTenantsCount(): number {
    return this.tenants.filter(t => t.activeLeasesCount > 0).length;
  }

  openCreateModal(): void {
    this.isEditMode = false;
    this.editingTenantId = null;
    this.currentTenant = {
      fullName: '',
      phone: '',
      email: '',
      idCardOrPassport: '',
      lineId: '',
      emergencyContact: '',
      note: '',
      status: 'Active'
    };
    this.isModalOpen = true;
  }

  openEditModal(tenant: TenantItem): void {
    this.isEditMode = true;
    this.editingTenantId = tenant.id;
    this.currentTenant = {
      fullName: tenant.fullName,
      phone: tenant.phone,
      email: tenant.email || '',
      idCardOrPassport: tenant.idCardOrPassport || '',
      lineId: tenant.lineId || '',
      emergencyContact: tenant.emergencyContact || '',
      note: tenant.note || '',
      status: tenant.status || 'Active'
    };
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
  }

  saveTenant(): void {
    if (!this.currentTenant.fullName || !this.currentTenant.phone) {
      alert('กรุณากรอกชื่อ-นามสกุล และเบอร์โทรศัพท์');
      return;
    }

    this.isSaving = true;

    if (this.isEditMode && this.editingTenantId) {
      const payload: UpdateTenantPayload = {
        fullName: this.currentTenant.fullName,
        phone: this.currentTenant.phone,
        email: this.currentTenant.email,
        idCardOrPassport: this.currentTenant.idCardOrPassport,
        lineId: this.currentTenant.lineId,
        emergencyContact: this.currentTenant.emergencyContact,
        status: this.currentTenant.status || 'Active',
        note: this.currentTenant.note
      };

      this.tenantsService.updateTenant(this.editingTenantId, payload).subscribe({
        next: () => {
          this.isSaving = false;
          this.closeModal();
          this.loadTenants();
        },
        error: (err) => {
          console.error('Failed to update tenant:', err);
          alert('ไม่สามารถอัปเดตข้อมูลผู้เช่าได้: ' + (err.error?.message || err.message));
          this.isSaving = false;
        }
      });
    } else {
      const payload: CreateTenantPayload = {
        fullName: this.currentTenant.fullName,
        phone: this.currentTenant.phone,
        email: this.currentTenant.email,
        idCardOrPassport: this.currentTenant.idCardOrPassport,
        lineId: this.currentTenant.lineId,
        emergencyContact: this.currentTenant.emergencyContact,
        note: this.currentTenant.note
      };

      this.tenantsService.createTenant(payload).subscribe({
        next: () => {
          this.isSaving = false;
          this.closeModal();
          this.loadTenants();
        },
        error: (err) => {
          console.error('Failed to create tenant:', err);
          alert('ไม่สามารถเพิ่มผู้เช่าได้: ' + (err.error?.message || err.message));
          this.isSaving = false;
        }
      });
    }
  }

  deleteTenant(id: string, name: string): void {
    if (!confirm(`คุณต้องการลบข้อมูลผู้เช่า "${name}" ใช่หรือไม่?`)) {
      return;
    }

    this.tenantsService.deleteTenant(id).subscribe({
      next: () => {
        this.loadTenants();
      },
      error: (err) => {
        console.error('Failed to delete tenant:', err);
        alert('ไม่สามารถลบข้อมูลผู้เช่าได้: ' + (err.error?.message || err.message));
      }
    });
  }

  getInitial(name: string): string {
    if (!name) return 'T';
    const clean = name.replace(/^คุณ|^นาย|^นาง|^นางสาว|^Mr\.|^Mrs\.|^Ms\./, '').trim();
    return clean.charAt(0).toUpperCase() || 'T';
  }
}
