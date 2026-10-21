import { inject } from '@angular/core';
import { ResolveFn } from '@angular/router';
import { OwnerSettlementsService, SettlementPipelineSummary } from './owner-settlements.service';

export const ownerSettlementsResolver: ResolveFn<SettlementPipelineSummary> = () => {
  return inject(OwnerSettlementsService).getSettlementsData();
};
