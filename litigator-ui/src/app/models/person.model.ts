export interface PersonName {
  title?: string;
  first: string;
  middle?: string;
  last?: string;
  suffix?: string;
  preferred?: string;
  full?: string;
  professional?: string;
  display?: string;
  lastFirst?: string;
  casual?: string;
}

export interface PersonAddress {
  id: number;
  address: Address;
  type: AddressType;
}

export interface Address {
  line1?: string;
  line2?: string;
  city?: string;
  county?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  isPopulated: boolean;
  display?: string;
  singleLine?: string;
  multiLine?: string;
}

export interface PersonPhoneNumber {
  id: number;
  phoneNumber: PhoneNumber;
  type: PhoneType;
}

export interface PhoneNumber {
  number?: string;
  extension?: string;
  isPopulated: boolean;
  formatted?: string;
  display?: string;
}

export enum AddressType {
  Primary = 0,
  Home = 1,
  Work = 2,
  Emergency = 3,
  Other = 99
}

export enum PhoneType {
  Primary = 0,
  Mobile = 1,
  Home = 2,
  Work = 3,
  Other = 99
}

export interface Person {
  systemId: number;
  personId: string;
  name: PersonName;
  email?: string;
  primaryAddress?: Address;
  primaryPhone?: PhoneNumber;
  createdDate: Date;
  modifiedDate?: Date;
  additionalAddresses: PersonAddress[];
  additionalPhones: PersonPhoneNumber[];
}

export interface LegalProfessional extends Person {
  barNumber: string;
  primaryAddress: Address;   // Required for legal professionals
  primaryPhone: PhoneNumber; // Required for legal professionals
  specialization: LegalSpecialization;
  isActive: boolean;
}

// Updated to match your backend enum
export enum LegalSpecialization {
  GeneralPractice = 0,
  CriminalLaw = 1,
  CivilLitigation = 2,
  FamilyLaw = 3,
  CorporateLaw = 4,
  RealEstateLaw = 5,
  PersonalInjury = 6,
  EmploymentLaw = 7,
  ImmigrationLaw = 8,
  BankruptcyLaw = 9,
  TaxLaw = 10,
  IntellectualProperty = 11
}

export enum PersonType {
  Client = 0,
  Attorney = 1,
  Judge = 2
}
