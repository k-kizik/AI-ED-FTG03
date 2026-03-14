import { useState, useEffect } from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { compareVersions } from '../api/comparisons';
import { getVersionFileUrl } from '../api/documents';
import type { CompareDocumentsResult } from '../types/api';
import { PdfPanel } from '../components/comparison/PdfPanel';
import { ChangesSidebar } from '../components/comparison/ChangesSidebar';
import { AnalysisPanel } from '../components/comparison/AnalysisPanel';
import { LoadingOverlay } from '../components/shared/LoadingOverlay';
import { useTextDiff } from '../hooks/useTextDiff';

function ResizeHandle({ onDrag }: { onDrag: (delta: number) => void }) {
  function handleMouseDown(e: React.MouseEvent) {
    e.preventDefault();
    let prevX = e.clientX;
    function onMouseMove(ev: MouseEvent) {
      onDrag(ev.clientX - prevX);
      prevX = ev.clientX;
    }
    function onMouseUp() {
      window.removeEventListener('mousemove', onMouseMove);
      window.removeEventListener('mouseup', onMouseUp);
      document.body.style.removeProperty('cursor');
      document.body.style.removeProperty('user-select');
    }
    document.body.style.cursor = 'col-resize';
    document.body.style.userSelect = 'none';
    window.addEventListener('mousemove', onMouseMove);
    window.addEventListener('mouseup', onMouseUp);
  }
  return (
    <div
      onMouseDown={handleMouseDown}
      className="w-1.5 shrink-0 cursor-col-resize bg-gray-200 hover:bg-blue-400 transition-colors z-10"
    />
  );
}

export function ComparisonPage() {
  const { id } = useParams<{ id: string }>();
  const location = useLocation();
  const navigate = useNavigate();

  const locationState = location.state as {
    v1?: string;
    v2?: string;
    versionLabel?: string;
    result?: CompareDocumentsResult;
  };

  // Result is passed via navigation state from the CompareBar mutation
  const [result, setResult] = useState<CompareDocumentsResult | null>(
    locationState?.result ?? null,
  );
  const [currentPage, setCurrentPage] = useState(1);
  const [confirmRegenerate, setConfirmRegenerate] = useState(false);
  const [originalUrl, setOriginalUrl] = useState<string | null>(null);
  const [updatedUrl, setUpdatedUrl] = useState<string | null>(null);
  const [origWidth, setOrigWidth] = useState(500);
  const [sidebarWidth, setSidebarWidth] = useState(288);

  useEffect(() => {
    if (!locationState?.v1) return;
    let blobUrl: string;
    getVersionFileUrl(locationState.v1)
      .then((u) => { blobUrl = u; setOriginalUrl(u); })
      .catch(console.error);
    return () => { if (blobUrl) URL.revokeObjectURL(blobUrl); };
  }, [locationState?.v1]);

  useEffect(() => {
    if (!locationState?.v2) return;
    let blobUrl: string;
    getVersionFileUrl(locationState.v2)
      .then((u) => { blobUrl = u; setUpdatedUrl(u); })
      .catch(console.error);
    return () => { if (blobUrl) URL.revokeObjectURL(blobUrl); };
  }, [locationState?.v2]);

  const textDiff = useTextDiff(originalUrl, updatedUrl);

  const regenerateMutation = useMutation({
    mutationFn: () => {
      if (!result) throw new Error('No result to regenerate');
      // We need the version IDs from the existing comparison result.
      // The comparisonId is in the URL; the version IDs are not returned by the API directly.
      // We re-use the stored result's first change's linked data — or we need to store them.
      // Since the API was called with forceRegenerate and the same IDs, we get them from state.
      if (!locationState?.v1 || !locationState?.v2) throw new Error('Version IDs not available for regeneration');
      return compareVersions({ originalVersionId: locationState.v1, newVersionId: locationState.v2, forceRegenerate: true });
    },
    onSuccess: (data) => {
      setResult(data);
      setConfirmRegenerate(false);
    },
  });

  if (!result) {
    return (
      <div className="min-h-screen flex items-center justify-center text-gray-400 flex-col gap-4">
        <p>Comparison data not found.</p>
        <button onClick={() => navigate(-1)} className="text-blue-600 text-sm hover:underline">← Go back</button>
      </div>
    );
  }

  return (
    <div className="flex flex-col h-screen overflow-hidden bg-gray-100">
      {regenerateMutation.isPending && (
        <LoadingOverlay message="Re-generating AI analysis… this may take up to a minute." />
      )}

      {/* Header */}
      <header className="bg-white border-b border-gray-200 px-5 py-3 flex items-center gap-4 shrink-0">
        <button
          onClick={() => navigate(-1)}
          className="text-sm text-gray-500 hover:text-gray-800 flex items-center gap-1"
        >
          ← Back
        </button>
        <div className="flex-1 min-w-0">
          <p className="text-sm font-semibold text-gray-800 truncate">
            {locationState?.versionLabel ?? `Comparison ${id?.slice(0, 8)}`}
          </p>
          <p className="text-xs text-gray-400">
            {result.wasGenerated ? 'Freshly generated' : 'Loaded from cache'}
          </p>
        </div>
        {locationState?.v1 && locationState?.v2 && (
          <button
            onClick={() => setConfirmRegenerate(true)}
            className="text-sm text-gray-500 hover:text-blue-600 border border-gray-200 rounded-lg px-3 py-1.5"
          >
            Regenerate
          </button>
        )}
      </header>

      {/* Confirm regenerate dialog */}
      {confirmRegenerate && (
        <div className="fixed inset-0 z-40 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-xl shadow-xl p-6 max-w-sm w-full mx-4">
            <h3 className="font-semibold mb-2">Regenerate Analysis?</h3>
            <p className="text-sm text-gray-600 mb-4">
              This will re-run the AI analysis, which may take up to a minute.
            </p>
            <div className="flex gap-3 justify-end">
              <button onClick={() => setConfirmRegenerate(false)} className="px-4 py-2 text-sm rounded-lg border hover:bg-gray-50">Cancel</button>
              <button
                onClick={() => regenerateMutation.mutate()}
                className="px-4 py-2 text-sm rounded-lg bg-blue-600 text-white hover:bg-blue-700"
              >
                Regenerate
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Main area: two PDF panels + sidebar */}
      <div className="flex-1 flex overflow-hidden">
        {/* Original PDF */}
        {locationState?.v1 ? (
          <div style={{ width: origWidth, minWidth: 180 }} className="shrink-0 overflow-hidden">
            <PdfPanel
              url={originalUrl}
              label="Original"
              currentPage={currentPage}
              onPageChange={setCurrentPage}
              changedItems={textDiff?.original}
              highlightColor="rgba(239,68,68,0.35)"
            />
          </div>
        ) : (
          <div style={{ width: origWidth, minWidth: 180 }} className="shrink-0 flex items-center justify-center text-gray-400 text-sm">
            Original PDF not available
          </div>
        )}

        <ResizeHandle onDrag={(d) => setOrigWidth((w) => Math.max(180, w + d))} />

        {/* New PDF */}
        {locationState?.v2 ? (
          <div className="flex-1 overflow-hidden" style={{ minWidth: 180 }}>
            <PdfPanel
              url={updatedUrl}
              label="New Version"
              currentPage={currentPage}
              onPageChange={setCurrentPage}
              changedItems={textDiff?.updated}
              highlightColor="rgba(34,197,94,0.4)"
            />
          </div>
        ) : (
          <div className="flex-1 flex items-center justify-center text-gray-400 text-sm" style={{ minWidth: 180 }}>
            New PDF not available
          </div>
        )}

        <ResizeHandle onDrag={(d) => setSidebarWidth((w) => Math.max(200, w - d))} />

        {/* Changes sidebar */}
        <div style={{ width: sidebarWidth, minWidth: 200 }} className="shrink-0 overflow-hidden">
          <ChangesSidebar changes={result.changes} />
        </div>
      </div>

      {/* AI Analysis panel (collapsible, pinned to bottom) */}
      <AnalysisPanel result={result} />
    </div>
  );
}
