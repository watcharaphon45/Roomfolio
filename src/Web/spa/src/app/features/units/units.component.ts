import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { UnitItem } from './units.service';

@Component({
  selector: 'app-units',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './units.component.html',
  styleUrl: './units.component.css'
})
export class UnitsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  units: UnitItem[] = [];

  ngOnInit(): void {
    this.units = this.route.snapshot.data['data'] as UnitItem[] || [];
  }
}
