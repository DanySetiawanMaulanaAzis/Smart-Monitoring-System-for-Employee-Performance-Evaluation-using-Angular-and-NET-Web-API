import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class DateFormatterService {

  // Format untuk API -> yyyy-MM-dd
  formatDateForApi(dateStr: string): string {
    if (!dateStr) return '';
    const parts = dateStr.split('-'); // input dari <input type="date"> = yyyy-MM-dd
    if (parts.length === 3) {
      return `${parts[0]}-${parts[1]}-${parts[2]}`;
    }
    return dateStr;
  }

  // Format untuk UI -> dd-MM-yyyy
  formatDateForUi(dateStr: string): string {
    if (!dateStr) return '';
    const parts = dateStr.split('-'); // yyyy-MM-dd
    if (parts.length === 3) {
      return `${parts[2]}-${parts[1]}-${parts[0]}`;
    }
    return dateStr;
  }
}
