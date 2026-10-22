import { inject } from '@angular/core';
import { ResolveFn } from '@angular/router';
import { ReportSummary, ReportsService } from './reports.service';

export const reportsResolver: ResolveFn<ReportSummary> = () => {
  return inject(ReportsService).getSummary();
};
