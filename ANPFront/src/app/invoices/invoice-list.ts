import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { API_BASE } from './invoice.api';
import { INVOICE_STATUSES, InvoiceStatus, InvoiceSummary } from './invoice.models';

type StatusFilter = InvoiceStatus | 'all';

@Component({
  selector: 'app-invoice-list',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, CurrencyPipe, DatePipe],
  template: `
    <section class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-semibold tracking-tight">Invoices</h1>
          <p class="text-sm text-slate-500">
            Loaded with <code class="rounded bg-slate-100 px-1">httpResource()</code> — refetches
            when the filter signal changes.
          </p>
        </div>
        <a
          routerLink="/invoices/new"
          class="rounded-lg bg-slate-900 px-4 py-2 text-sm font-medium text-white hover:bg-slate-700"
        >
          New invoice
        </a>
      </div>

      <!-- Status filter: writing this signal re-triggers the resource request. -->
      <div class="flex flex-wrap items-center gap-2">
        @for (option of filters; track option) {
          <button
            type="button"
            (click)="status.set(option)"
            class="rounded-full px-3 py-1 text-sm font-medium transition"
            [class]="
              status() === option
                ? 'bg-slate-900 text-white'
                : 'bg-white text-slate-600 ring-1 ring-slate-200 hover:bg-slate-100'
            "
          >
            {{ option === 'all' ? 'All' : option }}
          </button>
        }

        <span class="ml-auto text-sm text-slate-500">
          {{ count() }} invoice{{ count() === 1 ? '' : 's' }} ·
          {{ grandTotal() | currency: 'EUR' }}
        </span>
      </div>

      @if (invoices.isLoading()) {
        <p class="rounded-lg border border-slate-200 bg-white p-6 text-slate-500">Loading…</p>
      } @else if (invoices.error()) {
        <div class="rounded-lg border border-red-200 bg-red-50 p-6 text-red-700">
          <p class="font-medium">Couldn't reach the API.</p>
          <p class="text-sm">
            Start the backend (<code>dotnet run --project ANP.API</code>) at
            <code>{{ base }}</code> and make sure PostgreSQL is running.
          </p>
          <button
            type="button"
            (click)="invoices.reload()"
            class="mt-3 rounded-md bg-red-600 px-3 py-1.5 text-sm font-medium text-white"
          >
            Retry
          </button>
        </div>
      } @else {
        @let rows = invoices.value() ?? [];
        @if (rows.length === 0) {
          <p class="rounded-lg border border-slate-200 bg-white p-6 text-slate-500">
            No invoices for this filter.
          </p>
        } @else {
          <div class="overflow-hidden rounded-lg border border-slate-200 bg-white">
            <table class="w-full text-sm">
              <thead class="bg-slate-50 text-left text-slate-500">
                <tr>
                  <th class="px-4 py-2 font-medium">Number</th>
                  <th class="px-4 py-2 font-medium">Customer</th>
                  <th class="px-4 py-2 font-medium">Issued</th>
                  <th class="px-4 py-2 font-medium">Status</th>
                  <th class="px-4 py-2 text-right font-medium">Total</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100">
                @for (invoice of rows; track invoice.id) {
                  <tr class="hover:bg-slate-50">
                    <td class="px-4 py-2 font-mono text-xs">{{ invoice.number }}</td>
                    <td class="px-4 py-2">{{ invoice.customerName }}</td>
                    <td class="px-4 py-2 text-slate-500">
                      {{ invoice.issueDate | date: 'mediumDate' }}
                    </td>
                    <td class="px-4 py-2">
                      <span class="rounded-full px-2 py-0.5 text-xs font-medium" [class]="badge(invoice.status)">
                        {{ invoice.status }}
                      </span>
                    </td>
                    <td class="px-4 py-2 text-right font-medium">
                      {{ invoice.total | currency: 'EUR' }}
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      }
    </section>
  `,
})
export class InvoiceList {
  protected readonly base = inject(API_BASE);
  protected readonly filters: readonly StatusFilter[] = ['all', ...INVOICE_STATUSES];

  /** Source signal — the resource URL is derived from it, so setting it refetches. */
  protected readonly status = signal<StatusFilter>('all');

  protected readonly invoices = httpResource<InvoiceSummary[]>(() => {
    const status = this.status();
    return status === 'all'
      ? `${this.base}/invoices`
      : `${this.base}/invoices?status=${status}`;
  });

  protected readonly count = computed(() => this.invoices.value()?.length ?? 0);
  protected readonly grandTotal = computed(() =>
    (this.invoices.value() ?? []).reduce((sum, i) => sum + i.total, 0),
  );

  protected badge(status: InvoiceStatus): string {
    switch (status) {
      case 'Paid':
        return 'bg-emerald-100 text-emerald-700';
      case 'Sent':
        return 'bg-blue-100 text-blue-700';
      default:
        return 'bg-slate-100 text-slate-600';
    }
  }
}
