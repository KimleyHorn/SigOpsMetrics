import { Component, Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SignalInfo} from '../models/signal-info';

@Injectable({
  providedIn: 'root'
})
export class SignalsService {
  public signalInfos: SignalInfo[];
  baseUrl: string;

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrlInject: string) {
    this.baseUrl = baseUrlInject;
  }

  getData() {
    return this.http.get<SignalInfo[]>(this.baseUrl + 'signals/all');
  }
}
