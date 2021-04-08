import { Component, Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SignalInfo} from '../models/signal-info';
import { map, tap } from 'rxjs/operators';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SignalsService {
  public signalInfos: SignalInfo[];
  private _signals: BehaviorSubject<SignalInfo[]> = new BehaviorSubject<SignalInfo[]>([]);
  public signals = this._signals.asObservable();
  baseUrl: string;

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrlInject: string) {
    this.baseUrl = baseUrlInject;
  }

  getData() {
    return this.http.get<SignalInfo[]>(this.baseUrl + 'signals/all');
  }
}

