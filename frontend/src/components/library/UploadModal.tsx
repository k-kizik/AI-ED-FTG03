import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { uploadDocument } from '../../api/documents';
import type { DocumentDto } from '../../types/api';

interface Props {
  onClose: () => void;
  existingDocument?: DocumentDto;
}

export function UploadModal({ onClose, existingDocument }: Props) {
  const qc = useQueryClient();
  const [name, setName] = useState(existingDocument?.name ?? '');
  const [description, setDescription] = useState('');
  const [versionNumber, setVersionNumber] = useState('');
  const [file, setFile] = useState<File | null>(null);
  const [error, setError] = useState('');

  const mutation = useMutation({
    mutationFn: (form: FormData) => uploadDocument(form),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['documents'] });
      onClose();
    },
    onError: (err: unknown) => {
      const msg = (err as { response?: { data?: { detail?: string; title?: string } } })
        ?.response?.data?.detail ??
        (err as { response?: { data?: { title?: string } } })?.response?.data?.title ??
        'Upload failed';
      setError(msg);
    },
  });

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!file) { setError('Please select a PDF file.'); return; }
    if (!versionNumber.trim()) { setError('Version number is required.'); return; }
    if (!name.trim()) { setError('Document name is required.'); return; }
    setError('');
    const form = new FormData();
    form.append('file', file);
    form.append('name', name);
    form.append('description', description);
    form.append('versionNumber', versionNumber);
    if (existingDocument) form.append('existingDocumentId', existingDocument.id);
    mutation.mutate(form);
  }

  return (
    <div className="fixed inset-0 z-40 flex items-center justify-center bg-black/40">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-md mx-4 p-6">
        <h2 className="text-lg font-semibold mb-4">
          {existingDocument ? `Add Version — ${existingDocument.name}` : 'Upload Document'}
        </h2>
        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          {!existingDocument && (
            <div>
              <label className="block text-sm font-medium mb-1">Document Name *</label>
              <input
                className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="e.g. Service Agreement"
                required
              />
            </div>
          )}
          <div>
            <label className="block text-sm font-medium mb-1">Version Number *</label>
            <input
              className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              value={versionNumber}
              onChange={(e) => setVersionNumber(e.target.value)}
              placeholder="e.g. 1.0, Draft v2"
              required
            />
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">Description</label>
            <input
              className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Optional notes"
            />
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">PDF File *</label>
            <label className="flex items-center gap-3 cursor-pointer">
              <span className="px-3 py-2 text-sm rounded-lg border border-gray-300 bg-white hover:bg-gray-50 text-gray-700 whitespace-nowrap select-none">
                Choose file
              </span>
              <span className="text-sm text-gray-500 truncate">
                {file ? file.name : 'No file chosen'}
              </span>
              <input
                type="file"
                accept=".pdf"
                className="sr-only"
                onChange={(e) => setFile(e.target.files?.[0] ?? null)}
                required
              />
            </label>
          </div>
          {error && <p className="text-red-600 text-sm">{error}</p>}
          <div className="flex gap-3 justify-end pt-2">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm rounded-lg border hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={mutation.isPending}
              className="px-4 py-2 text-sm rounded-lg bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-50"
            >
              {mutation.isPending ? 'Uploading…' : 'Upload'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
