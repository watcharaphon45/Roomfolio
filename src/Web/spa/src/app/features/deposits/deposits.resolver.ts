import { inject } from '@angular/core';
import { ResolveFn } from '@angular/router';
import { DepositItem, DepositsService } from './deposits.service';

export const depositsResolver: ResolveFn<DepositItem[]> = () => {
  return inject(DepositsService).getDeposits();
};
