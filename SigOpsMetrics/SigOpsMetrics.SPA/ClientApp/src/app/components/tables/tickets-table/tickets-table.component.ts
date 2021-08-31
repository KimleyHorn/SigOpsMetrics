import { Component, OnInit } from '@angular/core';
import { BehaviorSubject, Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { Filter } from 'src/app/models/filter';
import { Metrics } from 'src/app/models/metrics';
import { FilterService } from 'src/app/services/filter.service';
import { MetricsService } from 'src/app/services/metrics.service';

@Component({
  selector: 'app-tickets-table',
  templateUrl: './tickets-table.component.html',
  styleUrls: ['./tickets-table.component.css']
})
export class TicketsTableComponent implements OnInit {
  private _filterSubscription: Subscription;
  private _metricsSubscription: Subscription;
  private _filter: Filter;
  private _data: any;
  private _dt: Date = new Date();

  tableColumns: string[] = ["task_Type", "outstanding"];
  tableDataSource = new BehaviorSubject([]);
  metrics: Metrics = new Metrics();
  outstandingTickets: number = 0;

  constructor(private _metricsService: MetricsService,
    private _filterService: FilterService) { }

  ngOnInit(): void {
    //get the current month
    let metricDate = (this._dt.getMonth() + 1) + '/' + this._dt.getFullYear();
    metricDate = '2/' + this._dt.getFullYear(); //temporary date

    this.metrics.measure = "ttyp";
    this.metrics.start = metricDate;
    this.metrics.end = metricDate;

    this._filterSubscription = this._filterService.filters.subscribe(filter => {
      this._filter = filter;
      this._loadData();
    });

    this._metricsSubscription = this._metricsService.getMetrics(this.metrics).subscribe(data => {
      this._data = data;
      this._loadData();
    });
  }

  ngOnDestroy(): void {
    this._filterSubscription.unsubscribe();
    this._metricsSubscription.unsubscribe();
  }

  private _loadData(){
    if(this._filter !== undefined && this._data !== undefined){

      let filteredData = this._data.filter(value => value['corridor'] === this._filter.zone_Group || this._filter.zone_Group === 'All')
                          .sort((a,b) => b['outstanding'] - a['outstanding']);

      this.outstandingTickets = filteredData.reduce((sum, current) => sum + current['outstanding'],0);

      this.tableDataSource.next(filteredData);
    }
  }
}
