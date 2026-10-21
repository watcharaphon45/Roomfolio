import { inject } from '@angular/core';
import { ResolveFn } from '@angular/router';
import { ReportItem, ReportsService } from './reports.service';

export const reportsResolver: ResolveFn<ReportItem[]> = () => {
  return inject(ReportsService).getReports();
};
