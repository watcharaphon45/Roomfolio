import { inject } from '@angular/core';
import { ResolveFn } from '@angular/router';
import { LeaseItem, LeasesService } from './leases.service';

export const leasesResolver: ResolveFn<LeaseItem[]> = () => {
  return inject(LeasesService).getLeases();
};
