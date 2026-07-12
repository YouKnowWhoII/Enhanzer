import { LocationDetail } from './location.model';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAtUtc: string;
  locations: LocationDetail[];
}
