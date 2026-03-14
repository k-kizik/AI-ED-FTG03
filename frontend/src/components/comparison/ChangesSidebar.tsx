import { useState } from 'react';
import type { KeyChangeDto } from '../../types/api';
import { SeverityBadge } from '../shared/SeverityBadge';

interface Props {
  changes: KeyChangeDto[];
}

function KeyChangeItem({ change }: { change: KeyChangeDto }) {
  const [open, setOpen] = useState(false);
  return (
    <div className="border-b border-gray-100">
      <button
        onClick={() => setOpen((v) => !v)}
        className="w-full text-left px-3 py-3 hover:bg-blue-50 transition-colors flex items-start justify-between gap-2"
      >
        <div className="flex-1 min-w-0">
          <p className="text-sm font-medium text-gray-800 truncate">{change.title || 'Untitled change'}</p>
          {change.severity && <span className="mt-1 inline-block"><SeverityBadge severity={change.severity} /></span>}
        </div>
        <svg
          className={`w-4 h-4 text-gray-400 shrink-0 mt-0.5 transition-transform ${open ? 'rotate-180' : ''}`}
          fill="none" viewBox="0 0 24 24" stroke="currentColor"
        >
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
        </svg>
      </button>
      {open && (
        <div className="px-3 pb-3 text-xs text-gray-600 space-y-1.5">
          {change.description && <p>{change.description}</p>}
          {change.impact && <p><span className="font-medium text-gray-700">Impact:</span> {change.impact}</p>}
          {change.recommendation && <p><span className="font-medium text-gray-700">Recommendation:</span> {change.recommendation}</p>}
        </div>
      )}
    </div>
  );
}

export function ChangesSidebar({ changes }: Props) {
  return (
    <div className="flex flex-col h-full overflow-hidden w-full border-l border-gray-200 bg-white">
      <div className="px-3 py-2 border-b border-gray-200 shrink-0">
        <h3 className="text-xs font-semibold text-gray-500 uppercase">Key Changes</h3>
        <p className="text-xs text-gray-400 mt-0.5">{changes.length} change{changes.length !== 1 ? 's' : ''} identified by AI</p>
      </div>
      <div className="flex-1 overflow-y-auto">
        {changes.length === 0 && (
          <p className="text-sm text-gray-400 px-3 py-4">No key changes identified.</p>
        )}
        {changes.map((change, i) => (
          <KeyChangeItem key={i} change={change} />
        ))}
      </div>
    </div>
  );
}

