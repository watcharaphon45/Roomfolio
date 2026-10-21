import { inject } from '@angular/core';
import { ResolveFn } from '@angular/router';
import { ExpenseItem, ExpensesService } from './expenses.service';

export const expensesResolver: ResolveFn<ExpenseItem[]> = () => {
  return inject(ExpensesService).getExpenses();
};
