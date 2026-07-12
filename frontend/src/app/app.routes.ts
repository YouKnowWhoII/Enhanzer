import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login.component').then(component => component.LoginComponent)
  },
  {
    path: 'purchase',
    canActivate: [authGuard],
    loadComponent: () => import('./features/purchase/purchase-bill.component').then(component => component.PurchaseBillComponent)
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'purchase'
  },
  {
    path: '**',
    redirectTo: 'purchase'
  }
];
