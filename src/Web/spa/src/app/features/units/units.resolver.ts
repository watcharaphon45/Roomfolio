import { inject } from '@angular/core';
import { ResolveFn } from '@angular/router';
import { UnitItem, UnitsService } from './units.service';

export const unitsResolver: ResolveFn<UnitItem[]> = () => {
  return inject(UnitsService).getUnits();
};
