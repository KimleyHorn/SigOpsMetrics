import { Injectable } from '@angular/core';
import { MatLegacySelectChange as MatSelectChange } from '@angular/material/legacy-select';
import { BehaviorSubject } from 'rxjs';
import { FilterService } from 'src/app/services/filter.service';

@Injectable({
  providedIn: 'root'
})
export class MetricSelectService {
  private _selectedMetric: BehaviorSubject<string> = new BehaviorSubject<string>(null);
  public selectedMetric = this._selectedMetric.asObservable();

  constructor(private filterService: FilterService) { }

  selectMetric(event: MatSelectChange){
    this._selectedMetric.next(event.value);
    this.filterService.updateFilterErrorState(1);
  }
}
