import { Routes } from '@angular/router';
import { Login } from './components/login/login';
import { EmployeeComponent } from './components/employee.component/employee.component';
import { EngineerComponent } from './components/engineer.component/engineer.component';
import { authGuard } from './core/guards/auth-guard';
import { Layout } from './components/layout/layout';

export const routes: Routes = [
    { path: 'login', component: Login },
  {
    path: '',
    component: Layout,
    canActivate: [authGuard],
    children: [
      { path: 'employee-dashboard', component: EmployeeComponent, data: { role: 'Employee' } },
      { path: 'engineer-dashboard', component: EngineerComponent, data: { role: 'Engineer' } }
    ]
  },
  { path: '**', redirectTo: 'login' }
];
