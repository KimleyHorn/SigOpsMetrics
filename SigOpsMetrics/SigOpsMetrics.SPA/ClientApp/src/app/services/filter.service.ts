import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Filter } from '../models/filter';
import { BehaviorSubject } from 'rxjs';
import { SignalInfo } from '../models/signal-info';
import { map } from 'rxjs/operators';
import { FormatService } from './format.service';

@Injectable({
  providedIn: 'root'
})
export class FilterService {
  private _errorState: number = 1; // 1 = primary, 2 = warn, 3 = disabled
  private _errorStateObs = new BehaviorSubject<number>(this._errorState);
  public errorState = this._errorStateObs.asObservable();

  private filter: Filter = new Filter();
  private _filters = new BehaviorSubject<Filter>(null);
  public filters = this._filters.asObservable();
  public isFiltering: BehaviorSubject<boolean> = new BehaviorSubject(false);

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

  private _counties: BehaviorSubject<string[]> = new BehaviorSubject<string[]>([]);
  public counties = this._counties.asObservable();
  public countyData: string[];

  private _cities: BehaviorSubject<string[]> = new BehaviorSubject<string[]>([]);
  public cities = this._cities.asObservable();
  public cityData: string[];

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrlInject: string, private _formatService: FormatService) {
    this.checkExistingFilter();
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

  public getCountiesFromDb() {
    return this.http.get<any[]>(this.baseUrl + 'signals/counties');
  }

  public getCitiesFromDb() {
    return this.http.get<any[]>(this.baseUrl + 'signals/cities');
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

  getCorridorsByFilter(){
    return this.http.get<string[]>(this.baseUrl + 'signals/corridorsbyfilter' + "?zoneGroup=" + this.filter.zone_Group + "&zone=" + 
                                   this.filter.zone + "&agency=" + this.filter.agency + "&county=" + this.filter.county + "&city=" + this.filter.city).pipe(
      map(response => {
        this.corridorData = response;
        return response.sort();
      })
    ).subscribe(response => this._corridors.next(response));;
  }

  getSubcorridors(){
    return this.http.get<string[]>(this.baseUrl + 'signals/subcorridors').pipe(
      map(response => {
        this.subcorridorData = response;
        return response;
      })
    ).subscribe(response => this._subcorridors.next(response));
  }

  getSubcorridorsByCorridor(corridor: string){
    return this.http.get<string[]>(this.baseUrl + 'signals/subcorridorsbycorridor/' +  encodeURIComponent(corridor)).pipe(
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

  getCounties() {
    return this.http.get<string[]>(this.baseUrl + 'signals/counties').pipe(
      map(response => {
        this.countyData = response;
        return response;
      })
    ).subscribe(response => this._counties.next(response));;
  }

  getCities() {
    return this.http.get<string[]>(this.baseUrl + 'signals/cities').pipe(
      map(response => {
        this.cityData = response;
        return response;
      })
    ).subscribe(response => this._cities.next(response));;
  }

  private _loadData(filter: Filter){
    this.getZoneGroups();
    this.getZonesByZoneGroup(filter.zone_Group);
    this.getCorridorsByFilter();
    this.getSubcorridors();
    this.getAgencies();
    this.getSignals();
    this.getCounties();
    this.getCities();
  }

  public setValue(key: string, value: any){
    if (value) {
      switch (key) {
        case "zone_Group":
          this.getZonesByZoneGroup(value);
          break;
        case "zone":
          break;
        case "corridor":
          this.getSubcorridorsByCorridor(value);
          break;
        default:
          break;
      }
    }
    this.filter[key] = value;
    this.isFiltering.next(true);
    this.getCorridorsByFilter();
  }

  public updateFilter(){
    this._filters.next(this.filter);
    this.isFiltering.next(false);
  }

  public resetFilter(){
    this.filter = new Filter();
    this._loadData(this.filter);
    //this.updateFilter();
    //this.isFiltering.next(false);
  }

  // public filterCorridors(){
  //   for (let key of Object.keys(this.filter)) {
  //     switch (key)
  //     {
  //       case 'zone_Group':
  //       case 'zone':
  //       case 'agency':
  //       case 'city':
  //       case 'county':
  //         this.corridors = this.signals.pipe(map(cors => {
  //           let corr = cors.filter(c => c[key] === this.filter[key]);
  //           return (corr.length > 0) ? corr['corridor'] : null;
  //         }));
  //         break;
  //       default:
  //           break;
  //     }      
  //   }
  // }

  public filterData(data: any, corridors: [] = []){
    let filteredData = data;

    for (let key of Object.keys(this.filter)) {
      if(this.filter[key] && this.filter[key] !== null && key !== 'month'){
        switch (key) {
          case 'zone_Group':
            filteredData = filteredData.filter(dataItem => {
              let cor;

              if(corridors.length === 0){
                cor = this.corridorData.filter(cor => cor === dataItem['corridor'])[0];
              } else {
                cor = corridors.filter(cor => cor === dataItem['corridor'])[0];
              }

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

  public getAverageData(data: any[]){
    let metric = {
      avg: data.reduce((a, b) => a + b.avg, 0) / data.length,
      delta: data.reduce((a, b) => a + b.delta, 0) / data.length,
    }

    return metric;
  }

  public saveCurrentFilter() {
    localStorage.setItem("filter", JSON.stringify(this.filter));
  }

  private checkExistingFilter() {
    let localStorageFilter = localStorage.getItem('filter');
    if (localStorageFilter) {
      this.filter = JSON.parse(localStorageFilter);
    }
  }

  public updateFilterErrorState(state: number){
    this._errorStateObs.next(state);
  }
}
