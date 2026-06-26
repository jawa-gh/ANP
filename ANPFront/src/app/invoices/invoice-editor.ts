import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  injectAsync,
  linkedSignal,
  signal,
} from '@angular/core';
import { CurrencyPipe, PercentPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import {
  applyEach,
  form,
  FormField,
  min,
  minLength,
  required,
  submit,
  validateHttp,
  validateTree,
} from '@angular/forms/signals';
import { InvoiceApi, API_BASE } from './invoice.api';
import {
  INVOICE_STATUSES,
  InvoiceForm,
  InvoiceRead,
  InvoiceStatus,
  LineItem,
  emptyInvoiceForm,
  emptyLineItem,
} from './invoice.models';
import { MoneyInput } from './money-input';

type TaxPreset = 'none' | 'reduced' | 'standard';
const TAX_RATES: Record<TaxPreset, number> = { none: 0, reduced: 0.1, standard: 0.2 };
const CREDIT_LIMIT = 50_000;

@Component({
  selector: 'app-invoice-editor',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormField, MoneyInput, RouterLink, CurrencyPipe, PercentPipe],
  templateUrl: './invoice-editor.html',
})
export class InvoiceEditor {
  // private readonly api = inject(InvoiceApi);

  private readonly invoiceApi = injectAsync(() => import('./invoice.api').then((m) => m.InvoiceApi));

  protected readonly base = inject(API_BASE);
  protected readonly statuses = INVOICE_STATUSES;
  protected readonly creditLimit = CREDIT_LIMIT;

  // create the form model signal with inital/empty values
  protected readonly model = signal(emptyInvoiceForm());

  // create the form by passing the previous created model signal to the form() function, along with a validation function that defines all the validation rules for the form fields (including cross-field and async rules)
  protected readonly invoiceForm = form(this.model, (path) => {
    required(path.customerName, { message: 'Customer name is required' });
    minLength(path.customerName, 2, { message: 'At least 2 characters' });
    required(path.issueDate, { message: 'Issue date is required' });

    // Conditional validation: notes become mandatory once the invoice is "Sent".
    required(path.notes, {
      when: ({ valueOf }) => valueOf(path.status) === 'Sent',
      message: 'Add a note before sending',
    });

    // At least one line item.
    minLength(path.lineItems, 1, { message: 'Add at least one line item' });

    // Per-row rules applied to every item in the array.
    applyEach(path.lineItems, (item) => {
      required(item.productCode, { message: 'Required' });
      required(item.description, { message: 'Required' });
      min(item.quantity, 1, { message: 'Min 1' });
      min(item.unitPrice, 0, { message: 'Must be ≥ 0' });

      // Signal Form Async validation against the API: 200 => code exists, 404 => unknown.
      validateHttp(item.productCode, {
        request: ({ value }) => {
          const code = value().trim();
          return code ? `${this.base}/products/${encodeURIComponent(code)}` : undefined;
        },
        onSuccess: () => undefined,
        onError: () => ({ kind: 'unknownProduct', message: 'Unknown product code' }),
        debounce: 400,
      });
    });

    // Cross-field (tree) rule: the grand total must stay within the credit limit.
    validateTree(path, ({ value }) => {
      const inv = value();
      const subtotal = inv.lineItems.reduce(
        (sum, li) => sum + (li.quantity || 0) * (li.unitPrice || 0),
        0,
      );
      const total = subtotal * (1 + this.taxRate());
      return total > CREDIT_LIMIT
        ? { kind: 'creditLimit', message: `Total exceeds the ${CREDIT_LIMIT} credit limit` }
        : undefined;
    });
  });

  // --- Tax: linkedSignal resets to the preset, but stays user-overridable. ---
  protected readonly taxPreset = signal<TaxPreset>('standard');
  protected readonly taxRate = linkedSignal(() => TAX_RATES[this.taxPreset()]);

  // --- Derived totals (computed), mirroring the backend calculation. ---
  protected readonly subtotal = computed(() =>
    this.invoiceForm()
      .value()
      .lineItems.reduce((sum, li) => sum + (li.quantity || 0) * (li.unitPrice || 0), 0),
  );
  protected readonly taxAmount = computed(
    () => Math.round(this.subtotal() * this.taxRate() * 100) / 100,
  );
  protected readonly total = computed(() => this.subtotal() + this.taxAmount());

  // --- Submission state. ---
  protected readonly saved = signal<InvoiceRead | null>(null);
  protected readonly serverError = signal<string | null>(null);

  protected addLine(): void {
    this.model.update((m) => ({ ...m, lineItems: [...m.lineItems, emptyLineItem()] }));
  }

  protected removeLine(index: number): void {
    this.model.update((m) => ({
      ...m,
      lineItems: m.lineItems.filter((_, i) => i !== index),
    }));
  }

  protected async save(event: Event): Promise<void> {
    event.preventDefault();
    
    this.serverError.set(null);

    // The submit() function only runs your async callback if the form is valid. It also handles the form's submission state automatically.
    await submit(this.invoiceForm, {
      action: async (f) => {
        const payload = { ...f().value(), taxRate: this.taxRate() };
        try {
          const api = await this.invoiceApi();
          const result = await firstValueFrom(api.create(payload));
          this.saved.set(result);
        } catch {
          this.serverError.set('Could not save — is the API running?');
        }
        return undefined;
      },
    });
  }

  protected reset(): void {
    this.saved.set(null);
    this.serverError.set(null);
    this.model.set(emptyInvoiceForm());
    this.taxPreset.set('standard');
  }

  /** Override the linked tax rate from a percentage input (0–100). */
  protected setTaxPercent(event: Event): void {
    const percent = Number((event.target as HTMLInputElement).value);
    this.taxRate.set((Number.isFinite(percent) ? percent : 0) / 100);
  }

  protected stateClass(valid: boolean): string {
    return valid ? 'bg-emerald-100 text-emerald-700' : 'bg-red-100 text-red-700';
  }
}
