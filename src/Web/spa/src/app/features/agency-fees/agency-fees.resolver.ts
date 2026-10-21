import { inject } from '@angular/core';
import { ResolveFn } from '@angular/router';
import { AgencyFeeItem, AgencyFeesService } from './agency-fees.service';

export const agencyFeesResolver: ResolveFn<AgencyFeeItem[]> = () => {
  return inject(AgencyFeesService).getAgencyFees();
};
