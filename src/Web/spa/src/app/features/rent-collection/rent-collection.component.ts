import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { RentCollectionSummary } from './rent-collection.service';

@Component({
  selector: 'app-rent-collection',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './rent-collection.component.html',
  styleUrl: './rent-collection.component.css'
})
export class RentCollectionComponent implements OnInit {
  private route = inject(ActivatedRoute);
  summary: RentCollectionSummary | null = null;

  ngOnInit(): void {
    this.summary = this.route.snapshot.data['data'] as RentCollectionSummary;
  }
}
