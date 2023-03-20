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
  tableColumns: string[] = ["name", "value", "unit"];
  tableDataSource = new BehaviorSubject([]);
  public filter: Filter = new Filter();
  private _filterSubscription: Subscription;
  private _metricsSubscription: Subscription;

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
    this._metricsSubscription.unsubscribe();
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
    metrics.dashboard = true;

    //Since the interception happens before this, set it to N/A here to be safe
    this.tableData.filter(item => item.name === td.name)[0].value = "N/A";

    this._metricsSubscription = this._metricsService.straightAverage(metrics, this.filter).subscribe(response => {

      if (response) {
        let formattedVal = this._formatService.formatData(response.avg, td.format, td.precision, true);
        var dataItem = this.tableData.filter(item => item.name === td.name)[0];
        if (dataItem.format === 'percent') {
          this.tableData.filter(item => item.name === td.name)[0]["unit"] = '%';
        }
        this.tableData.filter(item => item.name === td.name)[0].value = formattedVal;

      } else {
        this.tableData.filter(item => item.name === td.name)[0].value = "N/A";
      }
    });
  }
}
