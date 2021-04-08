import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class FormatService {

  constructor() { }

  public formatNumber(val:number, dec: number = 0): string{
    return Number(val.toFixed(dec)).toLocaleString();
  }

  public formatPercent(val:number, dec: number = 0): string{
    let newVal = this.formatNumber(val * 100, dec);
    return newVal + '%';
  }
}
