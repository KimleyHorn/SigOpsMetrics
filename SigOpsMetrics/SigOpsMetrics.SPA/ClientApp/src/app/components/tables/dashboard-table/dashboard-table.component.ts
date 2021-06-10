import { Component, Input, OnInit } from '@angular/core';
import { BehaviorSubject, Subscription } from 'rxjs';
import { Filter } from 'src/app/models/filter';
import { Metrics } from 'src/app/models/metrics';
import { TableData } from 'src/app/models/table-data';
import { FilterService } from 'src/app/services/filter.service';
import { FormatService } from 'src/app/services/format.service';
import { MetricsService } from 'src/app/services/metrics.service';

@Component({
  selector: 'app-dashboard-table',
  templateUrl: './dashboard-table.component.html',
  styleUrls: ['./dashboard-table.component.css']
})
export class DashboardTableComponent implements OnInit {
  @Input() tableTitle: string = "";
  @Input() tableData: TableData[] = [];
  tableColumns: string[] = ["name", "value"];
  tableDataSource = new BehaviorSubject([]);
  public filter: Filter = new Filter();
  private _filterSubscription: Subscription;
  private _metricSubscription: Subscription;

  constructor(private _formatService: FormatService,
    private _filterService: FilterService,
    private _metricsService: MetricsService) { }

  ngOnInit(): void {
    this._filterSubscription = this._filterService.filters.subscribe(filter => {
      this.filter = filter;
      this._loadData();
    });
  }

  ngOnDestroy(): void{
    this._filterSubscription.unsubscribe();
    this._metricSubscription.unsubscribe();
  }

  private _loadData(){
    this.tableDataSource.next(this.tableData);

    this.tableData.forEach(dataItem => {
      this._getValue(dataItem);
    });
  }

  private _getValue(td: TableData){
    let metrics = new Metrics();
    metrics.measure = td.measure;
    metrics.start = this.filter.month;
    metrics.end = this.filter.month;

    this._metricSubscription = this._metricsService.getMetrics(metrics).subscribe(data => {
      let val = this._filterService.getZoneGroupData(data)[td.metric];
      let formattedVal = this._formatService.formatData(val, td.format, td.precision);
      this.tableData.filter(item => item.name === td.name)[0].value = formattedVal;
    });
  }
}
