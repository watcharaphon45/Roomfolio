import { inject } from '@angular/core';
import { ResolveFn } from '@angular/router';
import { OwnerItem, OwnersService } from './owners.service';

export const ownersResolver: ResolveFn<OwnerItem[]> = () => {
  return inject(OwnersService).getOwners();
};
