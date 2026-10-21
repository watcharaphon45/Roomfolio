import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { LeaseItem } from './leases.service';

@Component({
  selector: 'app-leases',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './leases.component.html',
  styleUrl: './leases.component.css'
})
export class LeasesComponent implements OnInit {
  private route = inject(ActivatedRoute);
  leases: LeaseItem[] = [];

  ngOnInit(): void {
    this.leases = this.route.snapshot.data['data'] as LeaseItem[] || [];
  }
}
