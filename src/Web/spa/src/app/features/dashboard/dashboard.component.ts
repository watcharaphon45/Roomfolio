import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { DashboardMetrics } from './dashboard.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  private route = inject(ActivatedRoute);
  data: DashboardMetrics | null = null;

  ngOnInit(): void {
    this.data = this.route.snapshot.data['data'] as DashboardMetrics;
  }
}
