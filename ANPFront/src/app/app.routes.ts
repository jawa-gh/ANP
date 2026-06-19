import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'invoices' },
  {
    path: 'invoices',
    loadComponent: () => import('./invoices/invoice-list').then((m) => m.InvoiceList),
  },
  {
    path: 'invoices/new',
    loadComponent: () => import('./invoices/invoice-editor').then((m) => m.InvoiceEditor),
  },
];
