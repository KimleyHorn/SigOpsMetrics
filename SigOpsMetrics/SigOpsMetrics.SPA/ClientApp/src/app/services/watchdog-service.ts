import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WatchdogFilter } from '../models/watchdog-filter';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class WatchdogService {
  baseUrl: string;
  private _data = new BehaviorSubject<any>(null);
  public data = this._data.asObservable();

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrlInject: string) {
    this.baseUrl = baseUrlInject;
  }

  loadFilteredData(filter: WatchdogFilter): void {
    this.http.post<any>(this.baseUrl + 'watchdog/data', filter).pipe(
    ).subscribe(response => this._data.next(response));
  }
}
