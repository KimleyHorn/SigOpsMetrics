import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class MetricsService {
  private _baseUrl: string = environment.API_PATH;

  constructor(private http: HttpClient) { }

  getMetrics(source: string, level: string, interval: string, measure: string, start: string, end: string){
    return this.http.get<any[]>(this._baseUrl + 'metrics?source=' + source
                                                    + '&level=' + level 
                                                    + "&interval=" + interval 
                                                    + "&measure=" + measure 
                                                    + "&start="+ start 
                                                    + "&end="+ end);
  }
}
