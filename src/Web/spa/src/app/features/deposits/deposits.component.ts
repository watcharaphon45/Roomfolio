import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { DepositItem } from './deposits.service';

@Component({
  selector: 'app-deposits',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './deposits.component.html',
  styleUrl: './deposits.component.css'
})
export class DepositsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  deposits: DepositItem[] = [];

  ngOnInit(): void {
    this.deposits = this.route.snapshot.data['data'] as DepositItem[] || [];
  }
}
