import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { OrganizationSettings } from './settings.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.css'
})
export class SettingsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  settings: OrganizationSettings | null = null;

  ngOnInit(): void {
    this.settings = this.route.snapshot.data['data'] as OrganizationSettings;
  }
}
