import { inject } from '@angular/core';
import { ResolveFn } from '@angular/router';
import { RentCollectionService, RentCollectionSummary } from './rent-collection.service';

export const rentCollectionResolver: ResolveFn<RentCollectionSummary> = () => {
  return inject(RentCollectionService).getRentCollectionData();
};
