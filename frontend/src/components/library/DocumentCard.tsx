import { useState } from 'react';
import type { DocumentDto } from '../../types/api';
import type { SelectedVersion } from '../../hooks/useVersionSelection';
import { VersionRow } from './VersionRow';
import { UploadModal } from './UploadModal';

interface Props {
  doc: DocumentDto;
  isSelected: (versionId: string) => boolean;
  onToggle: (item: SelectedVersion) => void;
  showOwner?: boolean;
}

export function DocumentCard({ doc, isSelected, onToggle, showOwner }: Props) {
  const [expanded, setExpanded] = useState(false);
  const [addingVersion, setAddingVersion] = useState(false);

  return (
    <>
      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        <div
          className="flex items-center justify-between px-5 py-4 cursor-pointer hover:bg-gray-50 transition-colors"
          onClick={() => setExpanded((v) => !v)}
        >
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2">
              <h3 className="font-semibold text-gray-900 truncate">{doc.name}</h3>
              <span className="text-xs text-gray-400 shrink-0">{doc.versions.length} version{doc.versions.length !== 1 ? 's' : ''}</span>
            </div>
            {doc.description && <p className="text-sm text-gray-500 truncate mt-0.5">{doc.description}</p>}
            {showOwner && <p className="text-xs text-purple-600 mt-0.5">{doc.userEmail}</p>}
          </div>
          <svg
            className={`w-5 h-5 text-gray-400 transition-transform ${expanded ? 'rotate-180' : ''}`}
            fill="none" viewBox="0 0 24 24" stroke="currentColor"
          >
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
          </svg>
        </div>

        {expanded && (
          <div className="border-t border-gray-100">
            {doc.versions.length === 0 ? (
              <p className="px-5 py-4 text-sm text-gray-400">No versions yet.</p>
            ) : (
              <table className="w-full">
                <thead>
                  <tr className="text-xs text-gray-400 uppercase border-b border-gray-100">
                    <th className="px-4 py-2 text-left w-8"></th>
                    <th className="px-4 py-2 text-left">Version</th>
                    <th className="px-4 py-2 text-left">File</th>
                    <th className="px-4 py-2 text-left">Pages</th>
                    <th className="px-4 py-2 text-left">Size</th>
                    <th className="px-4 py-2 text-left">Uploaded</th>
                  </tr>
                </thead>
                <tbody>
                  {doc.versions.map((v) => (
                    <VersionRow
                      key={v.id}
                      version={v}
                      documentId={doc.id}
                      documentName={doc.name}
                      checked={isSelected(v.id)}
                      onToggle={() =>
                        onToggle({
                          versionId: v.id,
                          versionNumber: v.versionNumber,
                          documentId: doc.id,
                          documentName: doc.name,
                          fileName: v.fileName,
                        })
                      }
                    />
                  ))}
                </tbody>
              </table>
            )}
            <div className="px-5 py-3 border-t border-gray-100">
              <button
                onClick={() => setAddingVersion(true)}
                className="text-sm text-blue-600 hover:text-blue-800 font-medium"
              >
                + Add Version
              </button>
            </div>
          </div>
        )}
      </div>

      {addingVersion && (
        <UploadModal existingDocument={doc} onClose={() => setAddingVersion(false)} />
      )}
    </>
  );
}
