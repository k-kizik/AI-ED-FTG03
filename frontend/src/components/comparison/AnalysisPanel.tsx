import { useState } from 'react';
import type { CompareDocumentsResult } from '../../types/api';

interface Props {
  result: CompareDocumentsResult;
}

export function AnalysisPanel({ result }: Props) {
  const [open, setOpen] = useState(true);

  return (
    <div className="border-t border-gray-200 bg-white shrink-0">
      <button
        onClick={() => setOpen((v) => !v)}
        className="w-full flex items-center justify-between px-5 py-3 hover:bg-gray-50 transition-colors"
      >
        <span className="text-sm font-semibold text-gray-700">AI Analysis</span>
        <svg className={`w-4 h-4 text-gray-400 transition-transform ${open ? '' : 'rotate-180'}`} fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
        </svg>
      </button>
      {open && (
        <div className="px-5 pb-5 grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <h4 className="text-xs font-semibold text-gray-500 uppercase mb-2">Summary</h4>
            <p className="text-sm text-gray-700 whitespace-pre-wrap">{result.summary || '—'}</p>
          </div>
          <div>
            <h4 className="text-xs font-semibold text-gray-500 uppercase mb-2">Legal Implications</h4>
            <p className="text-sm text-gray-700 whitespace-pre-wrap">{result.legalImplications || '—'}</p>
          </div>
          <div>
            <h4 className="text-xs font-semibold text-gray-500 uppercase mb-2">Risk Assessment</h4>
            <p className="text-sm text-gray-700 whitespace-pre-wrap">{result.riskAssessment || '—'}</p>
          </div>
        </div>
      )}
    </div>
  );
}
