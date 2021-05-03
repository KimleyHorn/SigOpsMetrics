import { DatePipe } from '@angular/common';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class FormatService {

  constructor(private _datePipe: DatePipe) { }

  public formatNumber(val:number, dec: number = 0): string{
    return Number(val.toFixed(dec)).toLocaleString();
  }

  public formatPercent(val:number, dec: number = 0): string{
    let newVal = this.formatNumber(val * 100, dec);
    return newVal + '%';
  }

  public formatDate(val: any, format: string = 'M/yyyy'):string{
    return this._datePipe.transform(val, format);
  }

  public formatData(val: any, format: string = null, dec: number = 0): string{
    if(val === undefined){
      return "";
    }else{
      switch (format) {
        case "percent":
            return this.formatPercent(val, dec);
        default:
            return this.formatNumber(val, dec);
      }
    }
  }
}
