export type InvoiceStatus = 'Draft' | 'Sent' | 'Paid';

export const INVOICE_STATUSES: readonly InvoiceStatus[] = ['Draft', 'Sent', 'Paid'];

/** A single editable row of the invoice. Matches the backend `LineItemDto`. */
export interface LineItem {
  productCode: string;
  description: string;
  quantity: number;
  unitPrice: number;
}

/** The editable shape the Signal Form is built around (matches `InvoiceWriteDto`). */
export interface InvoiceForm {
  customerName: string;
  issueDate: string; // ISO yyyy-MM-dd, as produced by <input type="date">
  status: InvoiceStatus;
  taxRate: number; // 0..1
  notes: string;
  lineItems: LineItem[];
}

/** Full invoice returned by the API (`InvoiceReadDto`), including server totals. */
export interface InvoiceRead extends InvoiceForm {
  id: number;
  number: string;
  subtotal: number;
  taxAmount: number;
  total: number;
}

/** List-row projection (`InvoiceSummaryDto`). */
export interface InvoiceSummary {
  id: number;
  number: string;
  customerName: string;
  issueDate: string;
  status: InvoiceStatus;
  total: number;
}

export interface Product {
  code: string;
  name: string;
  defaultPrice: number;
}

export function emptyLineItem(): LineItem {
  return { productCode: '', description: '', quantity: 1, unitPrice: 0 };
}

export function emptyInvoiceForm(): InvoiceForm {
  return {
    customerName: '',
    issueDate: new Date().toISOString().slice(0, 10),
    status: 'Draft',
    taxRate: 0.2,
    notes: '',
    lineItems: [emptyLineItem()],
  };
}
