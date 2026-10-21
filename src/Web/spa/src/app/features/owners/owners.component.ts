import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { OwnerItem } from './owners.service';

@Component({
  selector: 'app-owners',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './owners.component.html',
  styleUrl: './owners.component.css'
})
export class OwnersComponent implements OnInit {
  private route = inject(ActivatedRoute);
  owners: OwnerItem[] = [];

  ngOnInit(): void {
    this.owners = this.route.snapshot.data['data'] as OwnerItem[] || [];
  }
}
