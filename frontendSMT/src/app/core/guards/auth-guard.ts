import { CanActivateFn } from '@angular/router';
import { Router } from '@angular/router';
import { inject } from '@angular/core';
import { Storage } from '../../services/storage';

export const authGuard: CanActivateFn = (route, state) => {

  const router = inject(Router);
  const storage = inject(Storage);
  const token = storage.getItem('authToken');
  const role = storage.getItem('role');

  if (!token) {
    router.navigate(['/login']);
    return false;
  }

  // cek role
  const expectedRole = route.data['role'];
  if (expectedRole && role !== expectedRole) {
    router.navigate(['/login']);
    return false;
  }

  return true;
};
