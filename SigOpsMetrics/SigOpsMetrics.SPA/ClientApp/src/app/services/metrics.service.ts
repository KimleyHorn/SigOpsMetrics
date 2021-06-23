import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Filter } from '../models/filter';
import { Metrics } from '../models/metrics';
import { FilterService } from './filter.service';

@Injectable({
  providedIn: 'root'
})
export class MetricsService {
  private _baseUrl: string = environment.API_PATH;
  private _dt: Date = new Date();
  private _filter: Filter = new Filter();

  constructor(private http: HttpClient, private _filterService: FilterService) { }

  private _setDefaultMetric(metrics){
    if(metrics.source === undefined){
      metrics.source = "main";
    }

    if(metrics.level === undefined){
      metrics.level = "cor";
    }

    if(metrics.interval === undefined){
      metrics.interval = "mo";
    }

    if(metrics.start === undefined){
      let splitMonth = this._filterService.splitMonth();

      metrics.start = (splitMonth[0] + 1) + '/' + splitMonth[1];
    }

    if(metrics.end === undefined){
      metrics.end = this._filter.month;
    }

    return metrics;
  }

  getMetrics(metrics: Metrics){
    metrics = this._setDefaultMetric(metrics);

    return this.http.get<any[]>(this._baseUrl + 'metrics?source=' + metrics.source
                                                    + '&level=' + metrics.level
                                                    + "&interval=" + metrics.interval
                                                    + "&measure=" + metrics.measure
                                                    + "&start="+ metrics.start
                                                    + "&end="+ metrics.end);
  }

  getSignalMetrics(metrics: Metrics){
    metrics = this._setDefaultMetric(metrics);

    return this.http.get<any[]>(this._baseUrl + 'metrics/signals?source=' + metrics.source
                                                    + '&level=' + metrics.level
                                                    + "&interval=" + metrics.interval
                                                    + "&measure=" + metrics.measure
                                                    + "&start="+ metrics.start
                                                    + "&end="+ metrics.end
                                                    + "&metric=" + metrics.field);
  }

  filterMetrics(metrics: Metrics, filter: Filter){

    let options = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }) };

    return this.http.post<any[]>(this._baseUrl + 'metrics/filter?source=' + metrics.source
                                                + "&measure=" + metrics.measure,
                                              filter,
                                              options);
  }

  // filterMetrics(metrics: Metrics, filter: Filter){
  //   return this.http.post<any[]>(this._baseUrl + 'metrics/filter/' + metrics.source
  //                                               + "/" + metrics.measure,
  //                                             JSON.stringify(filter));
  // }
}
