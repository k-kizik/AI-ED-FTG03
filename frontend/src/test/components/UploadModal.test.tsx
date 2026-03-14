import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi } from 'vitest';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { UploadModal } from '../../components/library/UploadModal';
import * as documentsApi from '../../api/documents';
import type { DocumentDto } from '../../types/api';

function renderModal(onClose = vi.fn(), existingDocument?: DocumentDto) {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } });
  return render(
    <QueryClientProvider client={qc}>
      <UploadModal onClose={onClose} existingDocument={existingDocument} />
    </QueryClientProvider>,
  );
}

function makeFile(name = 'contract.pdf', type = 'application/pdf') {
  return new File(['content'], name, { type });
}

describe('UploadModal', () => {
  afterEach(() => vi.restoreAllMocks());

  it('shows "Upload Document" title when no existingDocument', () => {
    renderModal();
    expect(screen.getByText('Upload Document')).toBeInTheDocument();
  });

  it('shows "Add Version" title when existingDocument provided', () => {
    const doc: DocumentDto = { id: 'd1', name: 'My Contract', description: '', userId: 'u1', userEmail: '', createdAt: '', updatedAt: '', versions: [] };
    renderModal(vi.fn(), doc);
    expect(screen.getByText(/Add Version/i)).toBeInTheDocument();
  });

  it('shows error when submitting without a file', async () => {
    renderModal();
    await userEvent.type(screen.getByPlaceholderText(/1.0/i), '1.0');
    await userEvent.type(screen.getByPlaceholderText(/Service Agreement/i), 'My Contract');
    fireEvent.submit(screen.getByRole('button', { name: /upload/i }).closest('form')!);

    expect(await screen.findByText(/select a PDF file/i)).toBeInTheDocument();
  });

  it('shows error when submitting without version number', async () => {
    renderModal();
    await userEvent.type(screen.getByPlaceholderText(/Service Agreement/i), 'My Contract');
    // attach file via hidden input
    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement;
    await userEvent.upload(fileInput, makeFile());
    fireEvent.submit(screen.getByRole('button', { name: /upload/i }).closest('form')!);

    expect(await screen.findByText(/Version number is required/i)).toBeInTheDocument();
  });

  it('shows error when submitting without document name', async () => {
    renderModal();
    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement;
    await userEvent.upload(fileInput, makeFile());
    await userEvent.type(screen.getByPlaceholderText(/1.0/i), '1.0');
    fireEvent.submit(screen.getByRole('button', { name: /upload/i }).closest('form')!);

    expect(await screen.findByText(/Document name is required/i)).toBeInTheDocument();
  });

  it('calls uploadDocument with correct FormData on valid submit', async () => {
    const mockUpload = vi.spyOn(documentsApi, 'uploadDocument').mockResolvedValue({
      documentId: 'd1',
      versionId: 'v1',
      name: 'My Contract',
      versionNumber: '1.0',
      pageCount: 5,
    });

    renderModal();
    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement;
    await userEvent.upload(fileInput, makeFile());
    await userEvent.type(screen.getByPlaceholderText(/Service Agreement/i), 'My Contract');
    await userEvent.type(screen.getByPlaceholderText(/1.0/i), '1.0');
    await userEvent.click(screen.getByRole('button', { name: /upload/i }));

    await waitFor(() => expect(mockUpload).toHaveBeenCalledOnce());
    const form: FormData = mockUpload.mock.calls[0][0] as FormData;
    expect(form.get('name')).toBe('My Contract');
    expect(form.get('versionNumber')).toBe('1.0');
    expect(form.get('file')).toBeInstanceOf(File);
  });

  it('calls onClose after successful upload', async () => {
    vi.spyOn(documentsApi, 'uploadDocument').mockResolvedValue({
      documentId: 'd1',
      versionId: 'v1',
      name: 'My Contract',
      versionNumber: '1.0',
      pageCount: 5,
    });

    const onClose = vi.fn();
    renderModal(onClose);
    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement;
    await userEvent.upload(fileInput, makeFile());
    await userEvent.type(screen.getByPlaceholderText(/Service Agreement/i), 'My Contract');
    await userEvent.type(screen.getByPlaceholderText(/1.0/i), '1.0');
    await userEvent.click(screen.getByRole('button', { name: /upload/i }));

    await waitFor(() => expect(onClose).toHaveBeenCalledOnce());
  });

  it('shows API error message on upload failure', async () => {
    vi.spyOn(documentsApi, 'uploadDocument').mockRejectedValue({
      response: { data: { detail: 'File too large' } },
    });

    renderModal();
    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement;
    await userEvent.upload(fileInput, makeFile());
    await userEvent.type(screen.getByPlaceholderText(/Service Agreement/i), 'My Contract');
    await userEvent.type(screen.getByPlaceholderText(/1.0/i), '1.0');
    await userEvent.click(screen.getByRole('button', { name: /upload/i }));

    expect(await screen.findByText('File too large')).toBeInTheDocument();
  });

  it('shows selected filename after file is picked', async () => {
    renderModal();
    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement;
    await userEvent.upload(fileInput, makeFile('my-contract.pdf'));

    expect(screen.getByText('my-contract.pdf')).toBeInTheDocument();
  });

  it('calls onClose when Cancel button is clicked', async () => {
    const onClose = vi.fn();
    renderModal(onClose);

    await userEvent.click(screen.getByRole('button', { name: /cancel/i }));

    expect(onClose).toHaveBeenCalledOnce();
  });
});
