import { Routes } from '@angular/router';
import { MainLayoutComponent } from './core/layout/main-layout.component';
import { dashboardResolver } from './features/dashboard/dashboard.resolver';
import { unitsResolver } from './features/units/units.resolver';
import { ownersResolver } from './features/owners/owners.resolver';
import { tenantsResolver } from './features/tenants/tenants.resolver';
import { leasesResolver } from './features/leases/leases.resolver';
import { rentCollectionResolver } from './features/rent-collection/rent-collection.resolver';
import { agencyFeesResolver } from './features/agency-fees/agency-fees.resolver';
import { ownerSettlementsResolver } from './features/owner-settlements/owner-settlements.resolver';
import { expensesResolver } from './features/expenses/expenses.resolver';
import { depositsResolver } from './features/deposits/deposits.resolver';
import { reportsResolver } from './features/reports/reports.resolver';
import { settingsResolver } from './features/settings/settings.resolver';

export const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
        resolve: { data: dashboardResolver }
      },
      {
        path: 'units',
        loadComponent: () => import('./features/units/units.component').then(m => m.UnitsComponent),
        resolve: { data: unitsResolver }
      },
      {
        path: 'owners',
        loadComponent: () => import('./features/owners/owners.component').then(m => m.OwnersComponent),
        resolve: { data: ownersResolver }
      },
      {
        path: 'tenants',
        loadComponent: () => import('./features/tenants/tenants.component').then(m => m.TenantsComponent),
        resolve: { data: tenantsResolver }
      },
      {
        path: 'leases',
        loadComponent: () => import('./features/leases/leases.component').then(m => m.LeasesComponent),
        resolve: { data: leasesResolver }
      },
      {
        path: 'rent-collection',
        loadComponent: () => import('./features/rent-collection/rent-collection.component').then(m => m.RentCollectionComponent),
        resolve: { data: rentCollectionResolver }
      },
      {
        path: 'agency-fees',
        loadComponent: () => import('./features/agency-fees/agency-fees.component').then(m => m.AgencyFeesComponent),
        resolve: { data: agencyFeesResolver }
      },
      {
        path: 'owner-settlements',
        loadComponent: () => import('./features/owner-settlements/owner-settlements.component').then(m => m.OwnerSettlementsComponent),
        resolve: { data: ownerSettlementsResolver }
      },
      {
        path: 'expenses',
        loadComponent: () => import('./features/expenses/expenses.component').then(m => m.ExpensesComponent),
        resolve: { data: expensesResolver }
      },
      {
        path: 'deposits',
        loadComponent: () => import('./features/deposits/deposits.component').then(m => m.DepositsComponent),
        resolve: { data: depositsResolver }
      },
      {
        path: 'reports',
        loadComponent: () => import('./features/reports/reports.component').then(m => m.ReportsComponent),
        resolve: { data: reportsResolver }
      },
      {
        path: 'settings',
        loadComponent: () => import('./features/settings/settings.component').then(m => m.SettingsComponent),
        resolve: { data: settingsResolver }
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
