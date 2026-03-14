// Auth
export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResult {
  token: string;
  userId: string;
  email: string;
  role: 'User' | 'Manager';
}

export interface RegisterRequest {
  email: string;
  password: string;
  role?: 0 | 1; // 0 = User, 1 = Manager
}

// Documents
export interface DocumentVersionDto {
  id: string;
  fileName: string;
  versionNumber: string;
  pageCount: number;
  fileSizeBytes: number;
  uploadedAt: string;
}

export interface DocumentDto {
  id: string;
  name: string;
  description: string;
  userId: string;
  userEmail: string;
  createdAt: string;
  updatedAt: string;
  versions: DocumentVersionDto[];
}

export interface GetDocumentsResult {
  documents: DocumentDto[];
}

export interface UploadDocumentResult {
  documentId: string;
  versionId: string;
  name: string;
  versionNumber: string;
  pageCount: number;
}

// Comparisons
export interface KeyChangeDto {
  title: string;
  description: string;
  impact: string;
  severity: string;
  recommendation: string;
}

export interface CompareDocumentsResult {
  comparisonId: string;
  summary: string;
  legalImplications: string;
  riskAssessment: string;
  changes: KeyChangeDto[];
  wasGenerated: boolean;
}

export interface CompareRequest {
  originalVersionId: string;
  newVersionId: string;
  forceRegenerate: boolean;
}

export interface ApiError {
  title: string;
  detail?: string;
  errors?: Record<string, string[]>;
  status: number;
}
