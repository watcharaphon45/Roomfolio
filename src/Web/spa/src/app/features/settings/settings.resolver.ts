import { inject } from '@angular/core';
import { ResolveFn } from '@angular/router';
import { OrganizationSettings, SettingsService } from './settings.service';

export const settingsResolver: ResolveFn<OrganizationSettings> = () => {
  return inject(SettingsService).getSettings();
};
