import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { AgencyFeeItem } from './agency-fees.service';

@Component({
  selector: 'app-agency-fees',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './agency-fees.component.html',
  styleUrl: './agency-fees.component.css'
})
export class AgencyFeesComponent implements OnInit {
  private route = inject(ActivatedRoute);
  fees: AgencyFeeItem[] = [];

  ngOnInit(): void {
    this.fees = this.route.snapshot.data['data'] as AgencyFeeItem[] || [];
  }
}
