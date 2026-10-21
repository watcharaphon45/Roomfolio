import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { SettlementPipelineSummary } from './owner-settlements.service';

@Component({
  selector: 'app-owner-settlements',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './owner-settlements.component.html',
  styleUrl: './owner-settlements.component.css'
})
export class OwnerSettlementsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  data: SettlementPipelineSummary | null = null;

  ngOnInit(): void {
    this.data = this.route.snapshot.data['data'] as SettlementPipelineSummary;
  }
}
