import { Person } from './person.model';
import { AttorneyDTO } from './attorney.model';
import { Court } from './court.model';
import { Deadline } from './deadline.model';

export interface Case {
  caseId: number;
  caseNumber: string;
  caseTitle: string;
  caseType: string;
  filingDate: Date;
  status: CaseStatus;
  estimatedValue?: number;
  currentCost?: number;
  clientId: number;
  assignedAttorneyId: number;
  assignedJudgeId?: number;
  courtId: number;
  client?: Person;
  assignedAttorney?: Person | AttorneyDTO;
  assignedJudge?: Person;
  court?: Court;
  deadlines: Deadline[];
  daysActive?: number;
}

export enum CaseStatus {
  Open = 'Open',
  Active = 'Active',
  Pending = 'Pending',
  Closed = 'Closed'
}
