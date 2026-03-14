import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import type { SelectedVersion } from '../../hooks/useVersionSelection';
import { compareVersions } from '../../api/comparisons';
import { LoadingOverlay } from '../shared/LoadingOverlay';

interface Props {
  selected: SelectedVersion[];
  onClear: () => void;
}

export function CompareBar({ selected, onClear }: Props) {
  const navigate = useNavigate();

  const mutation = useMutation({
    mutationFn: ({ v1, v2 }: { v1: string; v2: string }) =>
      compareVersions({ originalVersionId: v1, newVersionId: v2, forceRegenerate: false }),
    onSuccess: (data) => {
      onClear();
      navigate(`/compare/${data.comparisonId}`, {
        state: {
          result: data,
          v1: a.versionId,
          v2: b.versionId,
          versionLabel: `${a.documentName}: ${a.versionNumber} → ${b.versionNumber}`,
        },
      });
    },
  });

  if (selected.length !== 2) return null;

  const [a, b] = selected;
  const sameDoc = a.documentId === b.documentId;

  return (
    <>
      {mutation.isPending && (
        <LoadingOverlay message="Analyzing changes with AI… this may take up to a minute." />
      )}
      <div className="fixed bottom-0 inset-x-0 z-30 bg-white border-t border-gray-200 shadow-lg px-6 py-4 flex items-center justify-between gap-4">
        <div className="flex-1 min-w-0">
          <p className="text-sm font-medium text-gray-800 truncate">
            {a.documentName}: <span className="text-blue-600">{a.versionNumber}</span>
            {' '}&rarr;{' '}
            <span className="text-blue-600">{b.versionNumber}</span>
          </p>
          {!sameDoc && (
            <p className="text-xs text-red-500 mt-0.5">Versions must belong to the same document.</p>
          )}
          {mutation.isError && (
            <p className="text-xs text-red-500 mt-0.5">Comparison failed. Please try again.</p>
          )}
        </div>
        <div className="flex items-center gap-3 shrink-0">
          <button onClick={onClear} className="text-sm text-gray-500 hover:text-gray-700">
            Clear
          </button>
          <button
            disabled={!sameDoc || mutation.isPending}
            onClick={() => mutation.mutate({ v1: a.versionId, v2: b.versionId })}
            className="px-5 py-2 text-sm rounded-lg bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-50 font-medium"
          >
            Compare
          </button>
        </div>
      </div>
    </>
  );
}
