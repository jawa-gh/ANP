import { ChangeDetectionStrategy, Component, model } from '@angular/core';
import { FormValueControl, transformedValue } from '@angular/forms/signals';

/**
 * A custom Signal Forms control. Implementing {@link FormValueControl} lets it be
 * driven by the `[formField]` directive just like a native input.
 *
 * - `model()` exposes the two-way bound numeric value the form reads/writes.
 * - `transformedValue()` keeps a *string* UI value in sync with that number,
 *   reporting a parse error to the field when the text isn't numeric.
 */
@Component({
  selector: 'app-money-input',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="relative">
      <span class="pointer-events-none absolute left-2 top-1/2 -translate-y-1/2 text-slate-400">
        €
      </span>
      <input
        type="text"
        inputmode="decimal"
        [value]="raw()"
        (input)="raw.set(asString($event))"
        class="w-full rounded-md border border-slate-300 py-1.5 pl-6 pr-2 text-right tabular-nums focus:border-slate-500 focus:outline-none"
      />
    </div>
  `,
})
export class MoneyInput implements FormValueControl<number> {
  /** Required by the FormValueControl contract — the value the field binds to. */
  readonly value = model<number>(0);

  protected readonly raw = transformedValue(this.value, {
    parse: (raw: string) => {
      const trimmed = (raw ?? '').trim();
      if (trimmed === '') {
        return { value: 0 };
      }
      const num = Number(trimmed);
      return Number.isNaN(num)
        ? { error: { kind: 'parse', message: 'Enter a valid amount' } }
        : { value: num };
    },
    format: (value: number) => (Number.isFinite(value) ? String(value) : ''),
  });

  protected asString(event: Event): string {
    return (event.target as HTMLInputElement).value;
  }
}
