import { Component, ContentChild, Input, OnInit, TemplateRef, ɵclearResolutionOfComponentResourcesQueue } from '@angular/core';
import { Subscription } from 'rxjs';
import { Metrics } from 'src/app/models/metrics';
import { FilterService } from 'src/app/services/filter.service';
import { FormatService } from 'src/app/services/format.service';
import { MetricsService } from 'src/app/services/metrics.service';

@Component({
  selector: 'app-base-dashboard',
  templateUrl: './base-dashboard.component.html',
  styleUrls: ['./base-dashboard.component.css']
})
export class BaseDashboardComponent implements OnInit {
  @ContentChild(TemplateRef) template: TemplateRef<any>;

  private _filterSubscription: Subscription;
  private _metricsSubscription: Subscription;
  private _averageSubscription: Subscription;
  private _straightAvgSubscription: Subscription;

  @Input() graphMetrics: Metrics;
  @Input() metricLabel: string = '';
  @Input() metricField: string = '';
  @Input() metricDecimals: number = 0;
  metricValue: string = '';

  @Input() changeLabel: string = '';
  @Input() changeField: string = 'delta';
  changeValue: string = '';

  @Input() mapSettings: any;
  markers: any;

  corridors: any;
  data: any;
  averageData: any;
  filteredData: any;
  filterState: any;

  avg: any;
  delta: any;

  constructor(private _filterService: FilterService,
    private _metricsService: MetricsService,
    private _formatService: FormatService) { }

    ngOnInit(): void {
      this._filterSubscription = this._filterService.filters.subscribe(filter => {
        this.filterState = filter;
        this._metricsSubscription = this._metricsService.filterMetrics(this.graphMetrics, filter).subscribe(response => {
          this.data = response;
          this._loadData();
          this._metricsSubscription.unsubscribe();
        });

        this._averageSubscription = this._metricsService.averageMetrics(this.graphMetrics, filter).subscribe(response => {
          this.averageData = response;
          this._loadData();
          this._averageSubscription.unsubscribe();
        });

        this._straightAvgSubscription = this._metricsService.straightAverage(this.graphMetrics, filter).subscribe(res => {
          this.avg = res.avg;
          this.delta = res.delta;
          this.loadAvg();
          this._straightAvgSubscription.unsubscribe();
        });
      });
    }

    ngOnDestroy(): void {
      this._filterSubscription.unsubscribe();
      this._metricsSubscription.unsubscribe();
      this._averageSubscription.unsubscribe();
    }

    private _loadData(){
      if(this.data !== undefined && this.averageData !== undefined){
        this.filteredData = this.data;

        //get a list of distinct corridors
        if (this.filterState.zone_Group === 'All') {
          this.corridors = new Set(this.filteredData.filter(value => value['zone_Group'] !== null).map(data => data['zone_Group']));
        } else {
          this.corridors = new Set(this.filteredData.filter(value => value['corridor'] !== null).map(data => data['corridor']));
        }

    } // else {
    //   this.filteredData = [];
    //   this.averageData = [];
    // }
  }

  private loadAvg() {
    if (this.graphMetrics.formatType === "percent") {
      this.metricValue = this._formatService.formatPercent(this.avg, this.graphMetrics.formatDecimals);
    } else {
      this.metricValue = this._formatService.formatNumber(this.avg, this.graphMetrics.formatDecimals);
    }
        // Display N/A for "Change from prior period" when using a custom date range
    if (this._filterService.checkDateRange() === true) {
      this.changeValue = null;
    }
      else {
        this.changeValue = this._formatService.formatPercent(this.delta, 2);
      }
    }
}
