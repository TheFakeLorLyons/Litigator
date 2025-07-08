import { Person } from './person.model';
import { AttorneyDTO } from './attorney.model';
import { AddressDTO, PhoneNumberDTO } from './shared.model';

export interface ClientDTO {
  clientId: number;
  name: string;
  email?: string;
  primaryPhone?: string;
  primaryAddress?: string;
  isActive: boolean;
  createdDate: Date;
  totalCases: number;
  activeCases: number;
}

export interface ClientDetailDTO {
  clientId: number;
  // Name information
  displayName: string;
  fullName: string;
  firstName: string;
  lastName?: string;
  middleName?: string;
  title?: string;
  suffix?: string;
  preferredName?: string;
  // Contact information
  email?: string;
  primaryPhone?: string;
  primaryAddress?: string;
  // All addresses and phones
  allAddresses: AddressDTO[];
  allPhones: PhoneNumberDTO[];
  // Client-specific
  attorneys: AttorneyDTO[];
  notes?: string;
  isActive: boolean;
  createdDate: Date;
  modifiedDate?: Date;
  // Case statistics
  totalCases: number;
  activeCases: number;
  closedCases: number;
  lastCaseDate?: Date;
  mostRecentCaseTitle?: string;
}
