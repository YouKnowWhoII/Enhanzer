import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LocationDetail } from '../models/location.model';

@Injectable({ providedIn: 'root' })
export class LocationService {
  constructor(private readonly http: HttpClient) {}

  getLocations(): Observable<LocationDetail[]> {
    return this.http.get<LocationDetail[]>(`${environment.apiBaseUrl}/locations`);
  }
}
