import { Component, Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Filter } from '../models/filter';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class FilterService {
  private filt: Filter = new Filter();
  private _filters = new BehaviorSubject<Filter>(null);
  public filters = this._filters.asObservable();

  baseUrl: string;
  public signalGroups: Array<any> = [];

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrlInject: string) {
    this.baseUrl = baseUrlInject;

    //this.getSignalGroupsFromDb().subscribe(data => this.signalGroups = data);
  }

  // public getSignalGroups() {
  //   if (this.signalGroups.length === 0) {
  //     this.getSignalGroupsFromDb().subscribe(data => {
  //       this.signalGroups = data;
  //       return this.signalGroups;});
  //   }
  //   else {
  //     return this.signalGroups;
  //   }
  // }

  public setValue(key: string, value: any){
    this.filt[key] = value;
    this._filters.next(this.filt);
  }
  
  //Region
  public getSignalGroupsFromDb() {
    return this.http.get<any[]>(this.baseUrl + 'signals/zonegroups');
  }
  //District
  public getDistrictsFromDb() {
    return this.http.get<any[]>(this.baseUrl + 'signals/zones');
  }
  //Managing Agency
  public getAgenciesFromDb() {
    return this.http.get<any[]>(this.baseUrl + 'signals/agencies');
  }
  //County
  public getCountiesFromDb() {
    return this.http.get<any[]>(this.baseUrl + 'signals/agencies');
  }
  //City
  public getCitiesFromDb() {
    return this.http.get<any[]>(this.baseUrl + 'signals/agencies');
  }
  //Corridor
  public getCorridorsFromDb() {
    return this.http.get<any[]>(this.baseUrl + 'signals/corridors');
  }
}
