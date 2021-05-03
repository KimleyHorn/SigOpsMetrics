import { Component, Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Filter } from '../models/filter';
import { BehaviorSubject } from 'rxjs';
import { SignalsService } from './signals.service';
import { SignalInfo } from '../models/signal-info';
import { map } from 'rxjs/operators';
import { DatePipe } from '@angular/common';
import { FormatService } from './format.service';

@Injectable({
  providedIn: 'root'
})
export class FilterService {
  private filter: Filter = new Filter();
  private _filters = new BehaviorSubject<Filter>(null);
  public filters = this._filters.asObservable();

  baseUrl: string;
  public signalGroups: Array<any> = [];

  private _signals: BehaviorSubject<SignalInfo[]> = new BehaviorSubject<SignalInfo[]>([]);
  public signals = this._signals.asObservable();
  public signalData: SignalInfo[];

  private _zoneGroups: BehaviorSubject<string[]> = new BehaviorSubject<string[]>([]);
  public zoneGroups = this._zoneGroups.asObservable();
  public zoneGroupData: string[];

  private _zones: BehaviorSubject<string[]> = new BehaviorSubject<string[]>([]);
  public zones = this._zones.asObservable();
  public zoneData: string[];

  private _corridors: BehaviorSubject<string[]> = new BehaviorSubject<string[]>([]);
  public corridors = this._corridors.asObservable();
  public corridorData: string[];

  private _subcorridors: BehaviorSubject<string[]> = new BehaviorSubject<string[]>([]);
  public subcorridors = this._subcorridors.asObservable();
  public subcorridorData: string[];

  private _agencies: BehaviorSubject<string[]> = new BehaviorSubject<string[]>([]);
  public agencies = this._agencies.asObservable();
  public agencyData: string[];

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrlInject: string, private _formatService: FormatService) {
    this.baseUrl = baseUrlInject;
    this._filters.next(this.filter);
    this.getSignalGroupsFromDb().subscribe(data => this.signalGroups = data);

    this._loadData(this.filter);
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

  getSignals(){
    return this.http.get<SignalInfo[]>(this.baseUrl + 'signals/all').pipe(
      map(response => {
        this.signalData = response;
        return response;
      })
    ).subscribe(response => this._signals.next(response));
  }

  getZoneGroups(){
    return this.http.get<string[]>(this.baseUrl + 'signals/zonegroups').pipe(
      map(response => {
        this.zoneGroupData = response;
        return response;
      })
    ).subscribe(response => this._zoneGroups.next(response));
  }

  getZones(){
    return this.http.get<string[]>(this.baseUrl + 'signals/zones').pipe(
      map(response => {
        this.zoneData = response;
        return response;
      })
    ).subscribe(response => this._zones.next(response));;
  }

  getZonesByZoneGroup(zoneGroup: string){
    return this.http.get<string[]>(this.baseUrl + 'signals/zonesbyzonegroup/' + zoneGroup).pipe(
      map(response => {
        this.zoneData = response;
        return response;
      })
    ).subscribe(response => this._zones.next(response));;
  }

  getCorridors(){
    return this.http.get<string[]>(this.baseUrl + 'signals/corridors').pipe(
      map(response => {
        this.corridorData = response;
        return response;
      })
    ).subscribe(response => this._corridors.next(response));;
  }

  getCorridorsByZone(zone: string){
    return this.http.get<string[]>(this.baseUrl + 'signals/corridorsbyzone/' + zone).pipe(
      map(response => {
        this.corridorData = response;
        return response;
      })
    ).subscribe(response => this._corridors.next(response));;
  }

  getCorridorsByZoneGroup(zoneGroup: string){
    return this.http.get<string[]>(this.baseUrl + 'signals/corridorsbyzonegroup/' + zoneGroup).pipe(
      map(response => {
        this.corridorData = response;
        return response;
      })
    ).subscribe(response => this._corridors.next(response));;
  }

  getSubcorridors(){
    return this.http.get<string[]>(this.baseUrl + 'signals/subcorridors').pipe(
      map(response => {
        this.subcorridorData = response;
        return response;
      })
    ).subscribe(response => this._subcorridors.next(response));;
  }

  getSubcorridorsByCorridor(corridor: string){
    return this.http.get<string[]>(this.baseUrl + 'signals/subcorridorsbycorridor/' + corridor).pipe(
      map(response => {
        this.subcorridorData = response;
        return response;
      })
    ).subscribe(response => this._subcorridors.next(response));;
  }

  getAgencies(){
    return this.http.get<string[]>(this.baseUrl + 'signals/agencies').pipe(
      map(response => {
        this.agencyData = response;
        return response;
      })
    ).subscribe(response => this._agencies.next(response));;
  }

  private _loadData(filter: Filter){
    this.getZoneGroups();
    this.getZonesByZoneGroup(filter.zone_Group);
    this.getCorridorsByZoneGroup(filter.zone_Group);
    this.getSubcorridors();
    this.getAgencies();
    this.getSignals();
  }

  public setValue(key: string, value: any){
    switch (key) {
      case "zone_Group":
        this.getZonesByZoneGroup(value);
        this.getCorridorsByZoneGroup(value);
        break;
      case "zone":
        this.getCorridorsByZone(value);
      case "corridor":
        this.getSubcorridorsByCorridor(value);
      default:
        break;
    }

    this.filter[key] = value;
    //this._filters.next(this.filter);
  }

  public updateFilter(){
    this._filters.next(this.filter);
  }

  public filterData(data: any){
    let filteredData = data;
    for (let key of Object.keys(this.filter)) {
      if(this.filter[key] !== undefined && key !== 'month'){
        switch (key) {
          case 'zone_Group':
            filteredData = filteredData.filter(dataItem => {
              let cor = this.corridorData.filter(cor => cor === dataItem['corridor'])[0];
              if(dataItem['corridor'] === cor || dataItem['corridor'] === this.filter[key]){
                return dataItem;
              }
            });

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
    let groupData = data.filter(dataItem => {
      let dt = this._formatService.formatDate(dataItem['month']);
      if(dt === this.filter.month && dataItem['zone_Group'] === this.filter.zone_Group){
        return dataItem;
      }
    });

    return groupData[0];
  }
}
