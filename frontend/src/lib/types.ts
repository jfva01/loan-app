export interface SubmitApplicationRequest {
  firstName: string;
  lastName: string;
  address: string;
  state: string;
  companyName: string;
  ssn: string;
  requestedAmount: number
}

export interface SubmitApplicationResponse {
  approved: boolean;
  denialReason: string | null;
  applicationId: string | null;
  isReturningCustomer: boolean
}