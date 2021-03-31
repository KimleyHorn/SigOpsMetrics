import { Component, Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { Filter } from '../models/filter';

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

  public setValue(key: string, value: any){
    this.filt[key] = value;
    this._filters.next(this.filt);
  }
}
