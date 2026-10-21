import { inject } from '@angular/core';
import { ResolveFn } from '@angular/router';
import { DashboardMetrics, DashboardService } from './dashboard.service';

export const dashboardResolver: ResolveFn<DashboardMetrics> = () => {
  return inject(DashboardService).getDashboardData();
};
