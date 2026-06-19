import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  // The invoice pages depend on the live API, so render them on the client
  // rather than trying to reach the backend during prerendering.
  { path: 'invoices', renderMode: RenderMode.Client },
  { path: 'invoices/new', renderMode: RenderMode.Client },
  { path: '**', renderMode: RenderMode.Prerender },
];
