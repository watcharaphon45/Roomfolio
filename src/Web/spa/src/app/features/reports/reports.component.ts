import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ReportItem, ReportSummary, ReportsService } from './reports.service';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.css'
})
export class ReportsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private reportsService = inject(ReportsService);

  summary: ReportSummary = {
    totalUnits: 0,
    settlementCount: 0,
    totalPaidToOwners: 0,
    agencyCashHeld: 0,
    overdueCount: 0,
    overdueAmount: 0,
    expiringLeasesCount: 0
  };

  downloadingId: string | null = null;
  downloadingMaster = false;
  toastMessage = '';

  reports: ReportItem[] = [
    {
      id: 'owner-statements',
      title: 'Owner Statement (Excel / CSV)',
      desc: 'สรุปรายรับค่าเช่า ค่าใช้จ่ายหักได้ ค่าบริหาร และเงินสุทธิโอนเจ้าของห้องแต่ละราย',
      format: 'CSV / Excel',
      endpoint: 'owner-statements/export-csv',
      defaultFilename: 'owner-statements.csv'
    },
    {
      id: 'agency-cash',
      title: 'Agency Cash Held Report (Excel / CSV)',
      desc: 'รายงานตรวจสอบเงินที่นายหน้ารับแทนและยังถือครองอยู่ โดยไม่นับรวมยอดจ่ายตรง',
      format: 'CSV / Excel',
      endpoint: 'agency-cash/export-csv',
      defaultFilename: 'agency-cash-held.csv'
    },
    {
      id: 'rent-due',
      title: 'Rent Due & Outstanding (Excel / CSV)',
      desc: 'รายงานครบกำหนดชำระค่าเช่า ประวัติการรับเงินจริง และยอดค้างชำระแบ่งตามระยะเวลา',
      format: 'CSV / Excel',
      endpoint: 'rent-due/export-csv',
      defaultFilename: 'rent-due-outstanding.csv'
    },
    {
      id: 'lease-expiry',
      title: 'Lease Expiry 30/60/90 Days (Excel / CSV)',
      desc: 'รายงานคาดการณ์สัญญาหมดอายุ เพื่อเตือนต่อสัญญาหรือเตรียมปล่อยเช่าใหม่ล่วงหน้า',
      format: 'CSV / Excel',
      endpoint: 'lease-expiry/export-csv',
      defaultFilename: 'lease-expiry-90days.csv'
    }
  ];

  ngOnInit(): void {
    const resolved = this.route.snapshot.data['data'] as ReportSummary;
    if (resolved) {
      this.summary = resolved;
    } else {
      this.loadSummary();
    }
  }

  loadSummary(): void {
    this.reportsService.getSummary().subscribe({
      next: (res) => {
        this.summary = res;
      }
    });
  }

  onDownload(report: ReportItem): void {
    this.downloadingId = report.id;
    this.reportsService.downloadReport(report.endpoint).subscribe({
      next: (blob) => {
        const timestamp = new Date().toISOString().replace(/[-:T.]/g, '').slice(0, 14);
        const filename = `${report.id}-${timestamp}.csv`;
        this.reportsService.saveBlob(blob, filename);
        this.downloadingId = null;
        this.showToast(`ดาวน์โหลด ${report.title} สำเร็จแล้ว`);
      },
      error: () => {
        this.downloadingId = null;
        this.showToast('เกิดข้อผิดพลาดในการดาวน์โหลดรายงาน');
      }
    });
  }

  onDownloadMaster(): void {
    this.downloadingMaster = true;
    this.reportsService.downloadMaster().subscribe({
      next: (blob) => {
        const timestamp = new Date().toISOString().replace(/[-:T.]/g, '').slice(0, 14);
        this.reportsService.saveBlob(blob, `roomfolio-master-portfolio-${timestamp}.csv`);
        this.downloadingMaster = false;
        this.showToast('ดาวน์โหลดรายงาน Excel ภาพรวมพอร์ตโฟลิโอสำเร็จแล้ว');
      },
      error: () => {
        this.downloadingMaster = false;
        this.showToast('เกิดข้อผิดพลาดในการดาวน์โหลดรายงานภาพรวม');
      }
    });
  }

  private showToast(msg: string): void {
    this.toastMessage = msg;
    setTimeout(() => {
      this.toastMessage = '';
    }, 4000);
  }
}
