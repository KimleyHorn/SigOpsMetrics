import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ChartToggleService {
  private _toggleValue = new BehaviorSubject<string>(null);
  toggleValue = this._toggleValue;

  constructor() { }

  setValue(value){
    this._toggleValue.next(value);
  }
}
