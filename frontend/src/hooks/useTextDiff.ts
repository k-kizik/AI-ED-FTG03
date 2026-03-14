import { useEffect, useRef, useState } from 'react';
import { pdfjs } from 'react-pdf';
import { diffWords } from 'diff';
import type { TextItem, TextMarkedContent } from 'pdfjs-dist/types/src/display/api';

type PdfTextItem = TextItem | TextMarkedContent;

export type PageDiff = Map<number, Set<number>>; // pageNumber → Set<itemIndex>

interface TextDiffResult {
  original: PageDiff;
  updated: PageDiff;
}

export function useTextDiff(
  originalUrl: string | null,
  updatedUrl: string | null,
): TextDiffResult | null {
  const [result, setResult] = useState<TextDiffResult | null>(null);
  const prevUrls = useRef<[string | null, string | null]>([null, null]);

  useEffect(() => {
    if (!originalUrl || !updatedUrl) return;
    if (prevUrls.current[0] === originalUrl && prevUrls.current[1] === updatedUrl) return;
    prevUrls.current = [originalUrl, updatedUrl];

    computeDiff(originalUrl, updatedUrl)
      .then(setResult)
      .catch(console.error);
  }, [originalUrl, updatedUrl]);

  return result;
}

async function computeDiff(
  originalUrl: string,
  updatedUrl: string,
): Promise<TextDiffResult> {
  const [origPdf, updPdf] = await Promise.all([
    pdfjs.getDocument(originalUrl).promise,
    pdfjs.getDocument(updatedUrl).promise,
  ]);

  const numPages = Math.max(origPdf.numPages, updPdf.numPages);
  const originalDiff: PageDiff = new Map();
  const updatedDiff: PageDiff = new Map();

  for (let pageNum = 1; pageNum <= numPages; pageNum++) {
    const [origContent, updContent] = await Promise.all([
      pageNum <= origPdf.numPages
        ? origPdf.getPage(pageNum).then((p) => p.getTextContent())
        : Promise.resolve({ items: [] as PdfTextItem[], styles: {}, lang: null }),
      pageNum <= updPdf.numPages
        ? updPdf.getPage(pageNum).then((p) => p.getTextContent())
        : Promise.resolve({ items: [] as PdfTextItem[], styles: {}, lang: null }),
    ]);

    const origStrs: string[] = origContent.items.map((item) => 'str' in item ? item.str : '');
    const updStrs: string[] = updContent.items.map((item) => 'str' in item ? item.str : '');

    const origText = origStrs.join(' ');
    const updText = updStrs.join(' ');

    if (origText.trim() === updText.trim()) continue;

    const parts = diffWords(origText, updText);

    const changedOrig = findChangedItems(origStrs, parts, 'removed');
    const changedUpd = findChangedItems(updStrs, parts, 'added');

    if (changedOrig.size > 0) originalDiff.set(pageNum, changedOrig);
    if (changedUpd.size > 0) updatedDiff.set(pageNum, changedUpd);
  }

  return { original: originalDiff, updated: updatedDiff };
}

function findChangedItems(
  strs: string[],
  diffParts: Array<{ value: string; added?: boolean; removed?: boolean }>,
  targetType: 'removed' | 'added',
): Set<number> {
  // Collect character ranges in the source string that are marked as changed
  const changedRanges: Array<[number, number]> = [];
  let origPos = 0;
  let updPos = 0;

  for (const part of diffParts) {
    const len = part.value.length;
    if (part.removed) {
      if (targetType === 'removed') changedRanges.push([origPos, origPos + len]);
      origPos += len;
    } else if (part.added) {
      if (targetType === 'added') changedRanges.push([updPos, updPos + len]);
      updPos += len;
    } else {
      origPos += len;
      updPos += len;
    }
  }

  // Build character ranges for each item in strs (mirroring strs.join(' '))
  const changed = new Set<number>();
  let pos = 0;

  for (let i = 0; i < strs.length; i++) {
    const itemStart = pos;
    const itemEnd = pos + strs[i].length;
    pos += strs[i].length + 1; // +1 for the joining space

    if (itemEnd <= itemStart) continue; // empty item, skip

    for (const [changeStart, changeEnd] of changedRanges) {
      if (itemStart < changeEnd && itemEnd > changeStart) {
        changed.add(i);
        break;
      }
    }
  }

  return changed;
}
