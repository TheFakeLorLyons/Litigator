export interface Document {
  documentId: number;
  documentName: string;
  filePath: string;
  fileSize: number;
  documentType: string;
  description?: string;
  uploadDate: Date;
  uploadedBy: string;
  caseId: number;
}
