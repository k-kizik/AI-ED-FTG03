import { renderHook, act } from '@testing-library/react';
import { useVersionSelection } from '../../hooks/useVersionSelection';

const makeVersion = (versionId: string, documentId: string, extra?: object) => ({
  versionId,
  versionNumber: `v${versionId}`,
  documentId,
  documentName: `Doc ${documentId}`,
  fileName: `${versionId}.pdf`,
  ...extra,
});

describe('useVersionSelection', () => {
  it('starts with empty selection', () => {
    const { result } = renderHook(() => useVersionSelection());
    expect(result.current.selected).toHaveLength(0);
  });

  it('selects a version on toggle', () => {
    const { result } = renderHook(() => useVersionSelection());
    const v = makeVersion('v1', 'doc1');

    act(() => result.current.toggle(v));

    expect(result.current.selected).toHaveLength(1);
    expect(result.current.selected[0].versionId).toBe('v1');
  });

  it('deselects already-selected version', () => {
    const { result } = renderHook(() => useVersionSelection());
    const v = makeVersion('v1', 'doc1');

    act(() => result.current.toggle(v));
    act(() => result.current.toggle(v));

    expect(result.current.selected).toHaveLength(0);
  });

  it('allows selecting two versions from the same document', () => {
    const { result } = renderHook(() => useVersionSelection());
    const v1 = makeVersion('v1', 'doc1');
    const v2 = makeVersion('v2', 'doc1');

    act(() => result.current.toggle(v1));
    act(() => result.current.toggle(v2));

    expect(result.current.selected).toHaveLength(2);
  });

  it('resets selection when toggling version from different document', () => {
    const { result } = renderHook(() => useVersionSelection());
    const v1 = makeVersion('v1', 'doc1');
    const v2 = makeVersion('v2', 'doc2');

    act(() => result.current.toggle(v1));
    act(() => result.current.toggle(v2));

    expect(result.current.selected).toHaveLength(1);
    expect(result.current.selected[0].versionId).toBe('v2');
  });

  it('evicts oldest when a third version from same document is selected', () => {
    const { result } = renderHook(() => useVersionSelection());
    const v1 = makeVersion('v1', 'doc1');
    const v2 = makeVersion('v2', 'doc1');
    const v3 = makeVersion('v3', 'doc1');

    act(() => result.current.toggle(v1));
    act(() => result.current.toggle(v2));
    act(() => result.current.toggle(v3));

    expect(result.current.selected).toHaveLength(2);
    expect(result.current.selected.map((s) => s.versionId)).toEqual(['v2', 'v3']);
  });

  it('isSelected returns true for selected version', () => {
    const { result } = renderHook(() => useVersionSelection());
    const v = makeVersion('v1', 'doc1');

    act(() => result.current.toggle(v));

    expect(result.current.isSelected('v1')).toBe(true);
  });

  it('isSelected returns false for unselected version', () => {
    const { result } = renderHook(() => useVersionSelection());

    expect(result.current.isSelected('v1')).toBe(false);
  });

  it('clear empties the selection', () => {
    const { result } = renderHook(() => useVersionSelection());
    const v = makeVersion('v1', 'doc1');

    act(() => result.current.toggle(v));
    act(() => result.current.clear());

    expect(result.current.selected).toHaveLength(0);
  });

  it('replacing first with different-doc version then adding same-doc works', () => {
    const { result } = renderHook(() => useVersionSelection());
    const v1 = makeVersion('v1', 'doc1');
    const v2 = makeVersion('v2', 'doc2');
    const v3 = makeVersion('v3', 'doc2');

    act(() => result.current.toggle(v1));
    act(() => result.current.toggle(v2)); // resets to [v2]
    act(() => result.current.toggle(v3)); // adds to [v2, v3]

    expect(result.current.selected).toHaveLength(2);
    expect(result.current.selected.map((s) => s.versionId)).toEqual(['v2', 'v3']);
  });
});
