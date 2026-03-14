import client from './client';
import type { CompareDocumentsResult, CompareRequest } from '../types/api';

export async function compareVersions(data: CompareRequest): Promise<CompareDocumentsResult> {
  const res = await client.post<CompareDocumentsResult>('/comparisons/compare', data);
  return res.data;
}
