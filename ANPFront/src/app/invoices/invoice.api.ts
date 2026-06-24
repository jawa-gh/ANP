import { Injectable, InjectionToken, Service, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { InvoiceForm, InvoiceRead } from './invoice.models';

/**
 * Base URL of the ANP.API backend. Overridable via DI (e.g. in tests or for a
 * different environment). Defaults to the .NET dev server's http profile.
 */
export const API_BASE = new InjectionToken<string>('API_BASE', {
  providedIn: 'root',
  factory: () => 'http://localhost:5250/api',
});

// The old way
// @Injectable({ providedIn: 'root' })
// export class InvoiceApi {
//   private readonly http = inject(HttpClient);
//   readonly base = inject(API_BASE);

//   create(payload: InvoiceForm) {
//     return this.http.post<InvoiceRead>(`${this.base}/invoices`, payload);
//   }

//   update(id: number, payload: InvoiceForm) {
//     return this.http.put<InvoiceRead>(`${this.base}/invoices/${id}`, payload);
//   }
// }

// The new way
@Service()
export class InvoiceApi {
  private readonly http = inject(HttpClient);
  readonly base = inject(API_BASE);

  create(payload: InvoiceForm) {
    return this.http.post<InvoiceRead>(`${this.base}/invoices`, payload);
  }

  update(id: number, payload: InvoiceForm) {
    return this.http.put<InvoiceRead>(`${this.base}/invoices/${id}`, payload);
  }
}
