import { Component, Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class FilterService {

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

  public getAgenciesFromDb() {
    return this.http.get<any[]>(this.baseUrl + 'signals/agencies');
  }
}
