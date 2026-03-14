import { useState, useCallback } from 'react';
import type { DocumentVersionDto } from '../types/api';

export interface SelectedVersion {
  versionId: string;
  versionNumber: string;
  documentId: string;
  documentName: string;
  fileName: string;
}

export function useVersionSelection() {
  const [selected, setSelected] = useState<SelectedVersion[]>([]);

  const toggle = useCallback((item: SelectedVersion) => {
    setSelected((prev) => {
      const alreadySelected = prev.some((s) => s.versionId === item.versionId);
      if (alreadySelected) {
        return prev.filter((s) => s.versionId !== item.versionId);
      }
      // If from a different document, start fresh
      if (prev.length > 0 && prev[0].documentId !== item.documentId) {
        return [item];
      }
      // Max 2
      if (prev.length >= 2) {
        return [prev[1], item];
      }
      return [...prev, item];
    });
  }, []);

  const clear = useCallback(() => setSelected([]), []);

  const isSelected = useCallback(
    (versionId: string) => selected.some((s) => s.versionId === versionId),
    [selected],
  );

  return { selected, toggle, clear, isSelected };
}

export type { DocumentVersionDto };
