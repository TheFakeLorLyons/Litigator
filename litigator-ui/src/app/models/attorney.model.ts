import {
  LegalProfessional,
  LegalSpecialization,
  PersonName,
  Address,
  PhoneNumber,
  PersonAddress,
  PersonPhoneNumber,
  AddressType,
  PhoneType
} from './person.model';

export interface Attorney extends LegalProfessional {
  attorneyId: string;
  clients: Client[];
}

// DTOs that match the backend
export interface AttorneyDTO {
  attorneyId: number;
  personId: string;
  name: PersonName;
  email?: string;
  barNumber: string;
  specialization: LegalSpecialization;
  isActive: boolean;
  primaryAddress?: Address;
  primaryPhone?: PhoneNumber;
}

export interface AttorneyDetailDTO {
  attorneyId: number;
  personId: string;
  name: PersonName;
  email?: string;
  barNumber: string;
  specialization: LegalSpecialization;
  isActive: boolean;
  primaryAddress?: Address;
  primaryPhone?: PhoneNumber;
  additionalAddresses: PersonAddress[];
  additionalPhones: PersonPhoneNumber[];
  createdDate: Date;
  modifiedDate?: Date;
  clientIds?: number[];
}

// For convenience, re-export types from person.model using 'export type'
export type { PersonAddress, PersonPhoneNumber, AddressType, PhoneType } from './person.model';

export interface Client {
  systemId: number;
  clientId: string;
  name: PersonName;
  email?: string;
  isActive: boolean;
}
