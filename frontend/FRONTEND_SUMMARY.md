# Frontend - Legal Document Comparator

**React 18 + TypeScript SPA** for uploading, managing, and comparing legal PDF documents side-by-side with AI-powered analysis.

---

## Project Structure

```
src/
├── api/          # Axios API clients (auth, documents, comparisons)
├── components/
│   ├── comparison/   # AnalysisPanel, ChangesSidebar, PdfPanel
│   ├── layout/       # AppHeader
│   ├── library/      # DocumentCard, VersionRow, CompareBar, UploadModal
│   └── shared/       # ProtectedRoute, LoadingOverlay, SeverityBadge
├── context/      # AuthContext (JWT token + user state)
├── hooks/        # useDocuments, useVersionSelection, useTextDiff
├── pages/        # LoginPage, RegisterPage, LibraryPage, ComparisonPage
├── test/         # Vitest tests (components + hooks)
└── types/        # Shared TypeScript API interfaces
```

---

## Architecture Overview

- **Routing** — React Router v7. `ProtectedRoute` guards authenticated pages; `ManagerRoute` restricts manager-only views.
- **Auth** — JWT stored in `localStorage`; `AuthContext` provides user/token state app-wide; axios interceptor attaches the Bearer header automatically.
- **Data fetching** — TanStack Query v5 for server state (caching, invalidation, mutations).
- **PDF rendering** — `react-pdf` (PDF.js) renders document pages in side-by-side panels.
- **Diff highlighting** — `diff` package computes text diffs between extracted page text; custom `useTextDiff` hook + `customTextRenderer` overlay highlights additions/deletions on the PDF panels.
- **Styling** — Tailwind CSS v4 utility classes; no component library.
- **Resizable panels** — custom drag `ResizeHandle` component for the comparison split view.

---

## Technologies

| Category | Library | Version |
|---|---|---|
| Framework | React | ^18.3 |
| Language | TypeScript | ~5.6 |
| Build tool | Vite | ^5.4 |
| Styling | Tailwind CSS | ^4 |
| Routing | React Router | ^7 |
| Server state | TanStack Query | ^5 |
| HTTP client | Axios | ^1 |
| PDF rendering | react-pdf (PDF.js) | ^10.4 |
| Text diff | diff | ^8 |
| Tests | Vitest + Testing Library + happy-dom | ^2 |

---

## Running Locally

```bash
cd frontend
npm install
npm run dev        # http://localhost:5173
```

Backend must be running at `https://localhost:5001` (configured in `src/api/client.ts`).

---

## Tests

```bash
cd frontend
npm test           # run once
npm run test:watch # watch mode
```

35 tests — hooks (`useVersionSelection`, `authReducer`) + components (`ProtectedRoute`, `CompareBar`, `UploadModal`).
