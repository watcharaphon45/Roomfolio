import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { OrganizationSettings, SettingsService } from './settings.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.css'
})
export class SettingsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private settingsService = inject(SettingsService);

  settings: OrganizationSettings | null = null;
  saving = false;
  saveMessage = '';
  isError = false;

  ngOnInit(): void {
    const resolvedData = this.route.snapshot.data['data'] as OrganizationSettings;
    if (resolvedData) {
      this.settings = { ...resolvedData };
    } else {
      this.loadSettings();
    }
  }

  loadSettings(): void {
    this.settingsService.getSettings().subscribe({
      next: (data) => {
        this.settings = { ...data };
      },
      error: () => {
        this.showMessage('ไม่สามารถโหลดข้อมูลการตั้งค่าได้', true);
      }
    });
  }

  onSave(): void {
    if (!this.settings) return;
    this.saving = true;
    this.saveMessage = '';

    this.settingsService.updateSettings({
      companyName: this.settings.companyName,
      taxId: this.settings.taxId,
      defaultCurrency: this.settings.defaultCurrency,
      timezone: this.settings.timezone,
      address: this.settings.address,
      phone: this.settings.phone,
      email: this.settings.email
    }).subscribe({
      next: (res) => {
        this.settings = { ...res };
        this.saving = false;
        this.showMessage('บันทึกการตั้งค่าองค์กรสำเร็จเรียบร้อย', false);
      },
      error: () => {
        this.saving = false;
        this.showMessage('เกิดข้อผิดพลาดในการบันทึกข้อมูล กรุณาลองใหม่อีกครั้ง', true);
      }
    });
  }

  onUpgrade(): void {
    if (!confirm('ยืนยันการอัปเกรดเป็น Business Plan (฿1,490 / เดือน, โควตารองรับ 500 ห้อง) หรือไม่?')) {
      return;
    }

    this.settingsService.upgradePlan('Business Plan', 1490, 500).subscribe({
      next: (res) => {
        this.settings = { ...res };
        this.showMessage('อัปเกรดเป็น Business Plan สำเร็จ! โควตารองรับ 500 ห้อง', false);
      },
      error: () => {
        this.showMessage('เกิดข้อผิดพลาดในการอัปเกรดแพ็กเกจ', true);
      }
    });
  }

  private showMessage(msg: string, isError: boolean): void {
    this.saveMessage = msg;
    this.isError = isError;
    setTimeout(() => {
      this.saveMessage = '';
    }, 4500);
  }

  get quotaPercentage(): number {
    if (!this.settings || !this.settings.roomLimit) return 0;
    return Math.min(100, Math.round((this.settings.roomCount / this.settings.roomLimit) * 100));
  }
}
