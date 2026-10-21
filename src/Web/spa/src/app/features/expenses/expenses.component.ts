import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ExpenseItem } from './expenses.service';

@Component({
  selector: 'app-expenses',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './expenses.component.html',
  styleUrl: './expenses.component.css'
})
export class ExpensesComponent implements OnInit {
  private route = inject(ActivatedRoute);
  expenses: ExpenseItem[] = [];

  ngOnInit(): void {
    this.expenses = this.route.snapshot.data['data'] as ExpenseItem[] || [];
  }
}
