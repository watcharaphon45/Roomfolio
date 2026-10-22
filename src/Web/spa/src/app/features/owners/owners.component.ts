import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { OwnersService, OwnerItem, CreateOwnerPayload } from './owners.service';

@Component({
  selector: 'app-owners',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './owners.component.html',
  styleUrl: './owners.component.css'
})
export class OwnersComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private ownersService = inject(OwnersService);

  owners: OwnerItem[] = [];
  searchTerm = '';
  isLoading = false;
  isModalOpen = false;
  isEditMode = false;
  editingOwnerId: string | null = null;
  isSaving = false;

  currentOwner: CreateOwnerPayload = {
    fullName: '',
    phone: '',
    email: '',
    bankName: '',
    bankAccountNumber: '',
    lineId: ''
  };

  ngOnInit(): void {
    this.owners = this.route.snapshot.data['data'] as OwnerItem[] || [];
    this.loadOwners();
  }

  loadOwners(): void {
    this.isLoading = true;
    this.ownersService.getOwners(this.searchTerm).subscribe({
      next: (res) => {
        this.owners = res;
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
    this.loadOwners();
  }

  openCreateModal(): void {
    this.isEditMode = false;
    this.editingOwnerId = null;
    this.currentOwner = {
      fullName: '',
      phone: '',
      email: '',
      bankName: 'ธ.กสิกรไทย (KBANK)',
      bankAccountNumber: '',
      lineId: ''
    };
    this.isModalOpen = true;
  }

  openEditModal(owner: OwnerItem): void {
    this.isEditMode = true;
    this.editingOwnerId = owner.id;
    this.currentOwner = {
      fullName: owner.fullName,
      phone: owner.phone,
      email: owner.email || '',
      bankName: owner.bankName || '',
      bankAccountNumber: owner.bankAccountNumber || '',
      lineId: owner.lineId || ''
    };
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
  }

  saveOwner(): void {
    if (!this.currentOwner.fullName || !this.currentOwner.phone) {
      alert('กรุณากรอกชื่อ-นามสกุล และเบอร์โทรศัพท์');
      return;
    }

    this.isSaving = true;

    if (this.isEditMode && this.editingOwnerId) {
      this.ownersService.updateOwner(this.editingOwnerId, this.currentOwner).subscribe({
        next: () => {
          this.isSaving = false;
          this.closeModal();
          this.loadOwners();
        },
        error: (err) => {
          console.error('Failed to update owner:', err);
          alert('ไม่สามารถอัปเดตข้อมูลเจ้าของห้องได้: ' + (err.error?.message || err.message));
          this.isSaving = false;
        }
      });
    } else {
      this.ownersService.createOwner(this.currentOwner).subscribe({
        next: () => {
          this.isSaving = false;
          this.closeModal();
          this.loadOwners();
        },
        error: (err) => {
          console.error('Failed to create owner:', err);
          alert('ไม่สามารถเพิ่มเจ้าของห้องได้: ' + (err.error?.message || err.message));
          this.isSaving = false;
        }
      });
    }
  }

  deleteOwner(id: string, name: string): void {
    if (!confirm(`คุณต้องการลบข้อมูลเจ้าของ "${name}" ใช่หรือไม่?`)) {
      return;
    }

    this.ownersService.deleteOwner(id).subscribe({
      next: () => {
        this.loadOwners();
      },
      error: (err) => {
        console.error('Failed to delete owner:', err);
        alert('ไม่สามารถลบข้อมูลเจ้าของได้: ' + (err.error?.message || err.message));
      }
    });
  }

  getInitial(name: string): string {
    if (!name) return 'O';
    const clean = name.replace(/^คุณ|^นาย|^นาง|^นางสาว|^Mr\.|^Mrs\.|^Ms\./, '').trim();
    return clean.charAt(0).toUpperCase() || 'O';
  }
}
