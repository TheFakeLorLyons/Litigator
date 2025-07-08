export interface AddressDTO {
  id: number;
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
  type: AddressType;
}

export interface PhoneNumberDTO {
  id: number;
  number?: string;
  extension?: string;
  isPopulated: boolean;
  formatted?: string;
  display?: string;
  type: PhoneType;
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
