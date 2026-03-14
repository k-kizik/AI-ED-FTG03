import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { vi } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { CompareBar } from '../../components/library/CompareBar';
import * as comparisonsApi from '../../api/comparisons';
import type { SelectedVersion } from '../../hooks/useVersionSelection';

const mockNavigate = vi.fn();
vi.mock('react-router-dom', async (importOriginal) => {
  const actual = await importOriginal<typeof import('react-router-dom')>();
  return { ...actual, useNavigate: () => mockNavigate };
});

function makeVersion(versionId: string, documentId: string): SelectedVersion {
  return {
    versionId,
    versionNumber: `v${versionId}`,
    documentId,
    documentName: `Doc ${documentId}`,
    fileName: `${versionId}.pdf`,
  };
}

function renderCompareBar(selected: SelectedVersion[], onClear = vi.fn()) {
  const qc = new QueryClient({ defaultOptions: { mutations: { retry: false } } });
  render(
    <QueryClientProvider client={qc}>
      <MemoryRouter>
        <CompareBar selected={selected} onClear={onClear} />
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('CompareBar', () => {
  afterEach(() => vi.restoreAllMocks());

  it('renders nothing when fewer than 2 versions selected', () => {
    const { container } = (() => {
      const qc = new QueryClient();
      return render(
        <QueryClientProvider client={qc}>
          <MemoryRouter>
            <CompareBar selected={[makeVersion('v1', 'doc1')]} onClear={vi.fn()} />
          </MemoryRouter>
        </QueryClientProvider>,
      );
    })();
    expect(container.firstChild).toBeNull();
  });

  it('renders compare bar with version labels when 2 same-doc versions selected', () => {
    renderCompareBar([makeVersion('v1', 'doc1'), makeVersion('v2', 'doc1')]);

    expect(screen.getByText('Compare')).toBeInTheDocument();
    expect(screen.getByText('vv1')).toBeInTheDocument();
    expect(screen.getByText('vv2')).toBeInTheDocument();
  });

  it('disables Compare button when versions are from different documents', () => {
    renderCompareBar([makeVersion('v1', 'doc1'), makeVersion('v2', 'doc2')]);

    expect(screen.getByRole('button', { name: 'Compare' })).toBeDisabled();
    expect(screen.getByText(/must belong to the same document/i)).toBeInTheDocument();
  });

  it('enables Compare button when versions are from same document', () => {
    renderCompareBar([makeVersion('v1', 'doc1'), makeVersion('v2', 'doc1')]);

    expect(screen.getByRole('button', { name: 'Compare' })).not.toBeDisabled();
  });

  it('calls onClear when Clear button is clicked', () => {
    const onClear = vi.fn();
    renderCompareBar([makeVersion('v1', 'doc1'), makeVersion('v2', 'doc1')], onClear);

    fireEvent.click(screen.getByRole('button', { name: 'Clear' }));

    expect(onClear).toHaveBeenCalledOnce();
  });

  it('navigates to compare page on successful comparison', async () => {
    const mockCompareVersions = vi.spyOn(comparisonsApi, 'compareVersions').mockResolvedValue({
      comparisonId: 'cmp-123',
      summary: 'Summary',
      legalImplications: '',
      riskAssessment: 'Low',
      changes: [],
      wasGenerated: true,
    });

    const onClear = vi.fn();
    renderCompareBar([makeVersion('v1', 'doc1'), makeVersion('v2', 'doc1')], onClear);

    fireEvent.click(screen.getByRole('button', { name: 'Compare' }));

    await waitFor(() => expect(mockCompareVersions).toHaveBeenCalledOnce());
    await waitFor(() => expect(mockNavigate).toHaveBeenCalledWith(
      '/compare/cmp-123',
      expect.objectContaining({ state: expect.objectContaining({ result: expect.any(Object) }) }),
    ));
    expect(onClear).toHaveBeenCalledOnce();
  });

  it('shows error message when comparison fails', async () => {
    vi.spyOn(comparisonsApi, 'compareVersions').mockRejectedValue(new Error('Network error'));

    renderCompareBar([makeVersion('v1', 'doc1'), makeVersion('v2', 'doc1')]);

    fireEvent.click(screen.getByRole('button', { name: 'Compare' }));

    await waitFor(() => expect(screen.getByText(/Comparison failed/i)).toBeInTheDocument());
  });
});
