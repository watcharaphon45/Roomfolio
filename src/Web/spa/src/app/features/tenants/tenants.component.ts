import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { TenantItem } from './tenants.service';

@Component({
  selector: 'app-tenants',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tenants.component.html',
  styleUrl: './tenants.component.css'
})
export class TenantsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  tenants: TenantItem[] = [];

  ngOnInit(): void {
    this.tenants = this.route.snapshot.data['data'] as TenantItem[] || [];
  }
}
