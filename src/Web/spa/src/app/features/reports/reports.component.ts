import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ReportItem } from './reports.service';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.css'
})
export class ReportsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  reports: ReportItem[] = [];

  ngOnInit(): void {
    this.reports = this.route.snapshot.data['data'] as ReportItem[] || [];
  }
}
