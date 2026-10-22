import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { UnitsService, UnitItem, ProjectLookup, OwnerLookup, CreateUnitPayload } from './units.service';

@Component({
  selector: 'app-units',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './units.component.html',
  styleUrl: './units.component.css'
})
export class UnitsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private unitsService = inject(UnitsService);

  units: UnitItem[] = [];
  projects: ProjectLookup[] = [];
  owners: OwnerLookup[] = [];

  searchTerm = '';
  selectedStatus: number | null = null;
  isLoading = false;
  isModalOpen = false;
  isSaving = false;

  // New Unit Form
  newUnit: CreateUnitPayload = {
    projectId: '',
    ownerId: null,
    unitNumber: '',
    floor: 1,
    building: 'A',
    bedrooms: 1,
    bathrooms: 1,
    areaSqM: 35,
    targetRentPrice: 25000,
    status: 1, // Available
    paymentMode: 1, // DirectToOwner
    tenantName: '',
    note: ''
  };

  ngOnInit(): void {
    this.units = this.route.snapshot.data['data'] as UnitItem[] || [];
    this.loadLookups();
    this.loadUnits();
  }

  loadLookups(): void {
    this.unitsService.getProjects().subscribe(p => {
      this.projects = p;
      if (p.length > 0 && !this.newUnit.projectId) {
        this.newUnit.projectId = p[0].id;
      }
    });

    this.unitsService.getOwners().subscribe(o => {
      this.owners = o;
    });
  }

  loadUnits(): void {
    this.isLoading = true;
    this.unitsService.getUnits(this.searchTerm, this.selectedStatus ?? undefined).subscribe({
      next: (res) => {
        this.units = res;
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
    this.loadUnits();
  }

  filterByStatus(status: number | null): void {
    this.selectedStatus = status;
    this.loadUnits();
  }

  openCreateModal(): void {
    this.newUnit = {
      projectId: this.projects.length > 0 ? this.projects[0].id : '',
      ownerId: this.owners.length > 0 ? this.owners[0].id : null,
      unitNumber: '#',
      floor: 1,
      building: 'A',
      bedrooms: 1,
      bathrooms: 1,
      areaSqM: 35,
      targetRentPrice: 25000,
      status: 1,
      paymentMode: 1,
      tenantName: '',
      note: ''
    };
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
  }

  saveUnit(): void {
    if (!this.newUnit.unitNumber || !this.newUnit.projectId) {
      alert('กรุณากรอกเลขห้องและเลือกโครงการ');
      return;
    }

    this.isSaving = true;
    this.unitsService.createUnit(this.newUnit).subscribe({
      next: () => {
        this.isSaving = false;
        this.closeModal();
        this.loadUnits();
      },
      error: (err) => {
        console.error('Failed to create unit:', err);
        alert('เกิดข้อผิดพลาดในการบันทึกห้องชุด: ' + (err.error?.message || err.message));
        this.isSaving = false;
      }
    });
  }

  deleteUnit(id: string, unitNo: string): void {
    if (!confirm(`คุณต้องการลบห้อง ${unitNo} ใช่หรือไม่?`)) {
      return;
    }

    this.unitsService.deleteUnit(id).subscribe({
      next: () => {
        this.loadUnits();
      },
      error: (err) => {
        console.error('Failed to delete unit:', err);
        alert('ไม่สามารถลบห้องได้: ' + (err.error?.message || err.message));
      }
    });
  }
}
