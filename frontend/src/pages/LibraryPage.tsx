import { useState } from 'react';
import { AppHeader } from '../components/layout/AppHeader';
import { DocumentCard } from '../components/library/DocumentCard';
import { CompareBar } from '../components/library/CompareBar';
import { UploadModal } from '../components/library/UploadModal';
import { useDocuments } from '../hooks/useDocuments';
import { useVersionSelection } from '../hooks/useVersionSelection';

interface Props {
  allUsers?: boolean;
}

export function LibraryPage({ allUsers = false }: Props) {
  const { data: documents, isLoading, isError } = useDocuments(allUsers);
  const { selected, toggle, clear, isSelected } = useVersionSelection();
  const [uploading, setUploading] = useState(false);

  return (
    <div className="flex flex-col min-h-screen">
      <AppHeader />
      <main className="flex-1 px-6 py-6 max-w-5xl mx-auto w-full">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-xl font-bold text-gray-900">
              {allUsers ? 'Manager Dashboard' : 'Document Library'}
            </h1>
            {allUsers && (
              <p className="text-sm text-purple-600 mt-0.5">Showing documents from all users</p>
            )}
          </div>
          <button
            onClick={() => setUploading(true)}
            className="px-4 py-2 text-sm rounded-lg bg-blue-600 text-white hover:bg-blue-700 font-medium"
          >
            + Upload Document
          </button>
        </div>

        {isLoading && (
          <div className="flex justify-center py-12">
            <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
          </div>
        )}

        {isError && (
          <div className="bg-red-50 border border-red-200 rounded-lg px-5 py-4 text-red-700 text-sm">
            Failed to load documents. Make sure the backend is running.
          </div>
        )}

        {!isLoading && !isError && documents?.length === 0 && (
          <div className="text-center py-16 text-gray-400">
            <p className="text-lg">No documents yet.</p>
            <p className="text-sm mt-1">Click "Upload Document" to get started.</p>
          </div>
        )}

        <div className="space-y-3">
          {documents?.map((doc) => (
            <DocumentCard
              key={doc.id}
              doc={doc}
              isSelected={isSelected}
              onToggle={toggle}
              showOwner={allUsers}
            />
          ))}
        </div>
      </main>

      {/* Spacer so content isn't hidden behind the compare bar */}
      {selected.length === 2 && <div className="h-20" />}

      <CompareBar selected={selected} onClear={clear} />

      {uploading && <UploadModal onClose={() => setUploading(false)} />}
    </div>
  );
}
