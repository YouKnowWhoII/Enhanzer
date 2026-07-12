import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PurchaseBillSaveRequest, PurchaseBillSaveResponse } from '../models/purchase-item.model';

@Injectable({ providedIn: 'root' })
export class PurchaseBillService {
  constructor(private readonly http: HttpClient) {}

  save(request: PurchaseBillSaveRequest): Observable<PurchaseBillSaveResponse> {
    return this.http.post<PurchaseBillSaveResponse>(`${environment.apiBaseUrl}/purchase-bills`, request);
  }
}
