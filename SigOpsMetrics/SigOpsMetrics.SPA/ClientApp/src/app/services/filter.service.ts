import { Component, Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Filter } from '../models/filter';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class FilterService {
  private filter: Filter = new Filter();
  private _filters = new BehaviorSubject<Filter>(null);
  public filters = this._filters.asObservable();

  baseUrl: string;
  public signalGroups: Array<any> = [];

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrlInject: string) {
    this.baseUrl = baseUrlInject;
    this._filters.next(this.filter);
    this.getSignalGroupsFromDb().subscribe(data => this.signalGroups = data);
  }

  public getSignalGroups() {
    if (this.signalGroups.length === 0) {
      this.getSignalGroupsFromDb().subscribe(data => {
        this.signalGroups = data;
        return this.signalGroups;});
    }
    else {
      return this.signalGroups;
    }
  }

  public getSignalGroupsFromDb() {
    return this.http.get<any[]>(this.baseUrl + 'signals/zonegroups');
  }

  public getAgenciesFromDb() {
    return this.http.get<any[]>(this.baseUrl + 'signals/agencies');
  }

  public setValue(key: string, value: any){
    this.filter[key] = value;
    this._filters.next(this.filter);
  }

  public filterData(data: any){
    let filteredData = data;

    for (let key of Object.keys(this.filter)) {
      if(this.filter[key] !== undefined && key !== 'month'){
        switch (key) {
          case 'zone_Group':
            switch (this.filter[key]) {
              case 'All RTOP':
                filteredData = filteredData.filter(dataItem => dataItem[key] === this.filter[key] 
                  || dataItem[key] === 'RTOP1'
                  || dataItem[key] === 'RTOP2');
                break;
              case 'Zone 7':
                filteredData = filteredData.filter(dataItem => dataItem[key] === this.filter[key] 
                  || dataItem[key] === 'Zone 7m'
                  || dataItem[key] === 'Zone 7d');
                break;
              default:
                break;
            }
            break;
          default:
            filteredData = filteredData.filter(dataItem => dataItem[key] === this.filter[key]);
            break;
        }
      }
    }

    return filteredData;
  }

  public getZoneGroupData(data: any){
    // let dtObjects = this.filter.month.split('/');

    // let newMonth = Number(dtObjects[0]) - 1;
    // let newYear = Number(dtObjects[1]);
    // if(newMonth < 0){
    //     newMonth += 12;
    //     newYear = Number(dtObjects[1]) - 1;
    // }
    // let newDate = newMonth + "/" + newYear;

    let groupData = data.filter(dataItem => {
      let dt = (new Date(dataItem['month']).getMonth() + 1) + '/' + new Date(dataItem['month']).getFullYear();
      // if((dt === this.filter.month || dt === newDate)  && dataItem['zone_Group'] === this.filter.zone_Group){
      //   return dataItem;
      // }

      if(dt === this.filter.month && dataItem['zone_Group'] === this.filter.zone_Group){
        return dataItem;
      }
    });

    return groupData[0];
  }
}
