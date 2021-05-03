import { Injectable } from '@angular/core';
import { MatSelectChange } from '@angular/material/select';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MetricSelectService {
  private _selectedMetric: BehaviorSubject<string> = new BehaviorSubject<string>(null);
  public selectedMetric = this._selectedMetric.asObservable();

  constructor() { }

  selectMetric(event: MatSelectChange){
    this._selectedMetric.next(event.value);
  }
}
