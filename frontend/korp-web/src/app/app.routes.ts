import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'products',
  },
  {
    path: 'products',
    loadComponent: () =>
      import('./features/products/product-list/product-list').then((m) => m.ProductList),
  },
  {
    path: 'stock',
    loadComponent: () => import('./features/stock/stock-list/stock-list').then((m) => m.StockList),
  },
  {
    path: 'invoices',
    loadComponent: () =>
      import('./features/invoices/invoice-list/invoice-list').then((m) => m.InvoiceList),
  },
  {
    path: '**',
    redirectTo: 'products',
  },
];
