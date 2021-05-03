import { Component, OnInit } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Metrics } from 'src/app/models/metrics';
import { TableData } from 'src/app/models/table-data';
import { FilterService } from 'src/app/services/filter.service';
import { FormatService } from 'src/app/services/format.service';
import { MetricsService } from 'src/app/services/metrics.service';

@Component({
  selector: 'app-performance-table',
  templateUrl: './performance-table.component.html',
  styleUrls: ['./performance-table.component.css']
})
export class PerformanceTableComponent implements OnInit {
  performanceData: TableData[] = [];
  performanceColumns: string[] = ["name", "value"];
  performanceDataSource = new BehaviorSubject([]);

  constructor(private _formatService: FormatService,
    private _filterService: FilterService,
    private _metricsService: MetricsService) { }

  ngOnInit(): void {
    this._filterService.filters.subscribe(filter => {
      this._loadPerformanceData();
    });
  }

  private _loadPerformanceData(){
    this.performanceData = [
      { name: "Throughput", measure: "tp", metric: "vph", format:"number", precision: 0},
      { name: "Arrivals on Green", measure: "aogd", metric: "aog", format:"percent", precision: 1 },
      { name: "Progression Rate", measure: "prd", metric: "pr", format:"number", precision: 2 },
      { name: "Spillback Rate", measure: "qsd", metric: "qs_freq", format:"percent", precision: 1 },
      // {name: "Peak Period Split Failures", format:"percent", precision: 1, value:0 },
      // {name: "Off-Peak Split Failures", format:"percent", precision: 1, value: 0 },
      { name: "Travel Time Index", measure: "tti", metric: "tti", format:"number", precision: 2 },
      { name: "Planning Time Index", measure: "pti", metric: "pti", format:"number", precision: 2 },
    ];

    this.performanceDataSource.next(this.performanceData);

    this.performanceData.forEach(dataItem => {
      this._getValue(dataItem);
    });
  }

  private _getValue(td: TableData){
    let dt = new Date();
    let metrics = new Metrics();
    metrics.measure = td.measure;
    // metrics.start = (dt.getMonth() + 1) + '/' + dt.getFullYear();
    // metrics.end = (dt.getMonth() + 1) + '/' + dt.getFullYear();
    metrics.start = '3/' + dt.getFullYear();
    metrics.end = '3/' + dt.getFullYear();

    this._metricsService.getMetrics(metrics).subscribe(data => {
      let val = this._filterService.getZoneGroupData(data)[td.metric];
      let formattedVal = this._formatService.formatData(val, td.format, td.precision);
      this.performanceData.filter(item => item.name === td.name)[0].value = formattedVal;
    });
  }
}
