import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Filter } from '../models/filter';
import { Metrics } from '../models/metrics';
import { FilterService } from './filter.service';
import { AppConfig } from '../app.config';

@Injectable({
  providedIn: "root",
})
export class MetricsService {
  private _baseUrl: string = AppConfig.settings.API_PATH;
  private _dt: Date = new Date();
  private _filter: Filter = new Filter();
  private _options = {
    headers: new HttpHeaders({ "Content-Type": "application/json" }),
  };

  constructor(
    private http: HttpClient,
    private _filterService: FilterService
  ) {}

  private _setDefaultMetric(metrics) {
    if (metrics.source === undefined) {
      metrics.source = "main";
    }

    if (metrics.level === undefined) {
      metrics.level = "cor";
    }

    if (metrics.interval === undefined) {
      metrics.interval = "mo";
    }

    return metrics;
  }

  getMetrics(metrics: Metrics) {
    metrics = this._setDefaultMetric(metrics);

    return this.http.get<any[]>(
      this._baseUrl +
        "metrics?source=" +
        metrics.source +
        "&level=" +
        metrics.level +
        "&interval=" +
        metrics.interval +
        "&measure=" +
        metrics.measure +
        "&start=" +
        metrics.start +
        "&end=" +
        metrics.end
    );
  }

  getSignalMetrics(metrics: Metrics) {
    metrics = this._setDefaultMetric(metrics);

    return this.http.get<any[]>(
      this._baseUrl +
        "metrics/signals?source=" +
        metrics.source +
        "&level=" +
        metrics.level +
        "&interval=" +
        metrics.interval +
        "&measure=" +
        metrics.measure +
        "&start=" +
        metrics.start +
        "&end=" +
        metrics.end +
        "&metric=" +
        metrics.field
    );
  }

  filterMetrics(metrics: Metrics, filter: Filter) {
    return this.http.post<any[]>(
      this._baseUrl +
        "metrics/filter?source=" +
        metrics.source +
        "&measure=" +
        metrics.measure,
      filter,
      this._options
    );
  }

  summaryTrend(metrics: Metrics, filter: Filter) {
    return this.http.post<any[]>(
      this._baseUrl + "metrics/summarytrends?source=" + metrics.source,
      filter,
      this._options
    );
  }

  averageMetrics(metrics: Metrics, filter: Filter) {
    return this.http.post<any[]>(
      this._baseUrl +
        "metrics/average?source=" +
        metrics.source +
        "&measure=" +
        metrics.measure +
        "&dashboard=" +
        metrics.dashboard,
      filter,
      this._options
    );
  }

  filterSignalMetrics(metrics: Metrics, filter: Filter) {
    return this.http.post<any[]>(
      this._baseUrl +
        "metrics/signals/filter/average?source=" +
        metrics.source +
        "&measure=" +
        metrics.measure,
      filter,
      this._options
    );
  }

  straightAverage(metrics: Metrics, filter: Filter) {
    return this.http.post<any>(
      this._baseUrl +
      "metrics/straightaverage?source=" +
      metrics.source +
      "&measure=" +
      metrics.measure,
      filter,
      this._options
    );
  }

  averagesForMonth(zoneGroup: string, month: string) {
    return this.http.get<any[]>(
      this._baseUrl +
        "metrics/monthaverages?zoneGroup=" +
        zoneGroup +
        "&month=" +
        month
    );
  }
}
