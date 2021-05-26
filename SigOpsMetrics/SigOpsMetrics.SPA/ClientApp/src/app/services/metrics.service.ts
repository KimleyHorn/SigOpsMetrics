import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Metrics } from '../models/metrics';

@Injectable({
  providedIn: 'root'
})
export class MetricsService {
  private _baseUrl: string = environment.API_PATH;
  private _dt: Date = new Date();

  constructor(private http: HttpClient) { }

  getMetrics(metrics: Metrics){
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
      metrics.start = (this._dt.getMonth() + 1) + '/' + (this._dt.getFullYear() - 1);
    }

    if(metrics.end === undefined){
      metrics.end = (this._dt.getMonth() + 1) + '/' + this._dt.getFullYear();
    }

    return this.http.get<any[]>(this._baseUrl + 'metrics?source=' + metrics.source
                                                    + '&level=' + metrics.level
                                                    + "&interval=" + metrics.interval
                                                    + "&measure=" + metrics.measure
                                                    + "&start="+ metrics.start
                                                    + "&end="+ metrics.end);
  }
}
