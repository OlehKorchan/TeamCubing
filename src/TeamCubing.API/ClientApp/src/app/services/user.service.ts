import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigurationService } from '../shared/services/configuration.service';
import { UserStatisticsResponse } from '../models/userStatisticsResponse';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private readonly endpoint: string = '/account';
  private readonly userResultsEndpoint: string = '/results';

  public constructor(private httpClient: HttpClient, private config: ConfigurationService) {}

  public getCurrentUserResults(): Observable<UserStatisticsResponse> {
    return this.httpClient.get<UserStatisticsResponse>(
      this.config.getApiUrl() + this.endpoint + this.userResultsEndpoint,
    );
  }
}
