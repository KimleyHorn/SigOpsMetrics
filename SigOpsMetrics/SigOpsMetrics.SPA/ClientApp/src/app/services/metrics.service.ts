import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Metrics } from '../models/metrics';

@Injectable({
  providedIn: 'root'
})
export class MetricsService {
  private _baseUrl: string = environment.API_PATH;

  constructor(private http: HttpClient) { }

  getMetrics(metrics: Metrics){
    return this.http.get<any[]>(this._baseUrl + 'metrics?source=' + metrics.source
                                                    + '&level=' + metrics.level 
                                                    + "&interval=" + metrics.interval 
                                                    + "&measure=" + metrics.measure 
                                                    + "&start="+ metrics.start 
                                                    + "&end="+ metrics.end);
  }
}
