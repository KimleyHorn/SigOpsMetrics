import { Component, Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SignalInfo} from '../models/signal-info';
import { map } from 'rxjs/operators';
import { AppConfig } from '../app.config';
@Injectable({
  providedIn: 'root'
})
export class SignalsService {
  public signalInfos: SignalInfo[];
  baseUrl: string = AppConfig.settings.API_PATH;

  constructor(private http: HttpClient) {
  }

  getData() {
    return this.http.get<SignalInfo[]>(this.baseUrl + 'signals/all');
  }

}

