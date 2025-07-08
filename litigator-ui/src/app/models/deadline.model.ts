export interface Deadline {
  deadlineId: number;
  deadlineType: string;
  description?: string;
  notes?: string;
  deadlineDate: Date;
  completedDate?: Date;
  isCompleted: boolean;
  isCritical: boolean;
  caseId: number;
  daysUntil?: number;
  createdDate?: Date;
  modifiedDate?: Date;
}
