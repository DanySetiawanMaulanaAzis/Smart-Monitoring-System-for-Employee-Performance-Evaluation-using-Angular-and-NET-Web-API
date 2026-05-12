import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';



export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('authToken');

  if (token) {
    const cloned = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`)
    });
    return next(cloned);
  }

  return next(req);
};
