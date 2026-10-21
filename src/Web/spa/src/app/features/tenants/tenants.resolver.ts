import { inject } from '@angular/core';
import { ResolveFn } from '@angular/router';
import { TenantItem, TenantsService } from './tenants.service';

export const tenantsResolver: ResolveFn<TenantItem[]> = () => {
  return inject(TenantsService).getTenants();
};
