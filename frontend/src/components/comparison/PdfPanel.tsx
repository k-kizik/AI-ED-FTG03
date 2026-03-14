import { useState } from 'react';
import { Document, Page, pdfjs } from 'react-pdf';
import 'react-pdf/dist/Page/AnnotationLayer.css';
import 'react-pdf/dist/Page/TextLayer.css';
import type { PageDiff } from '../../hooks/useTextDiff';

// Use local worker from node_modules
pdfjs.GlobalWorkerOptions.workerSrc = new URL(
  'pdfjs-dist/build/pdf.worker.min.mjs',
  import.meta.url,
).toString();

interface Props {
  url: string | null;
  label: string;
  currentPage: number;
  onPageChange: (page: number) => void;
  changedItems?: PageDiff;
  highlightColor?: string;
}

export function PdfPanel({
  url,
  label,
  currentPage,
  onPageChange,
  changedItems,
  highlightColor = 'rgba(239,68,68,0.35)',
}: Props) {
  const [numPages, setNumPages] = useState(0);

  const makeTextRenderer = (pageNumber: number) => {
    const changed = changedItems?.get(pageNumber);
    if (!changed) return undefined;
    return (props: any) => {
      if (changed.has(props.itemIndex)) {
        return `<span style="background-color:${highlightColor};border-radius:2px;">${props.str}</span>`;
      }
      return props.str;
    };
  };

  if (!url) return <div className="flex-1 flex items-center justify-center text-gray-400 text-sm">Loading PDF…</div>;

  return (
    <div className="flex flex-col h-full overflow-hidden">
      <div className="flex items-center justify-between px-3 py-2 bg-gray-50 border-b border-gray-200 shrink-0">
        <span className="text-xs font-medium text-gray-600">{label}</span>
        <div className="flex items-center gap-2">
          <button
            disabled={currentPage <= 1}
            onClick={() => onPageChange(currentPage - 1)}
            className="text-gray-500 hover:text-gray-800 disabled:opacity-30 text-sm px-1"
          >
            ‹
          </button>
          <span className="text-xs text-gray-600">{currentPage} / {numPages || '?'}</span>
          <button
            disabled={currentPage >= numPages}
            onClick={() => onPageChange(currentPage + 1)}
            className="text-gray-500 hover:text-gray-800 disabled:opacity-30 text-sm px-1"
          >
            ›
          </button>
        </div>
      </div>
      <div className="flex-1 overflow-auto flex justify-center relative">
        <div className="relative inline-block">
          <Document
            file={url}
            onLoadSuccess={({ numPages: n }) => setNumPages(n)}
          >
            <Page
              pageNumber={currentPage}
              width={500}
              customTextRenderer={makeTextRenderer(currentPage)}
            />
          </Document>
        </div>
      </div>
    </div>
  );
}
