import client from './client';
import type { GetDocumentsResult, UploadDocumentResult } from '../types/api';

export async function getDocuments(includeAllUsers = false): Promise<GetDocumentsResult> {
  const res = await client.get<GetDocumentsResult>('/documents', {
    params: { includeAllUsers },
  });
  return res.data;
}

export async function uploadDocument(form: FormData): Promise<UploadDocumentResult> {
  const res = await client.post<UploadDocumentResult>('/documents/upload', form, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
  return res.data;
}

/** Returns a blob URL that can be passed directly to react-pdf. Revoke when done. */
export async function getVersionFileUrl(versionId: string): Promise<string> {
  const res = await client.get(`/documents/versions/${versionId}/file`, {
    responseType: 'blob',
  });
  return URL.createObjectURL(res.data as Blob);
}
