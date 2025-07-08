import { Person } from './person.model';
export interface Court {
  courtId: number;
  courtName: string;
  email?: string;
  county: string;
  state: string;
  courtType?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  postalCode?: string;
  country?: string;
  chiefJudgeId?: number;
  chiefJudge?: Person;
  createdDate?: Date;
  modifiedDate?: Date;
}
