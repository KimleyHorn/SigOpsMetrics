import { Component, ContentChild, Input, OnInit, TemplateRef } from '@angular/core';
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

  @Input() graphMetrics: Metrics;
  @Input() metricLabel: string = '';
  @Input() metricField: string = '';
  @Input() metricDecimals: number = 0;
  metricValue: string = '';

  @Input() changeLabel: string = '';
  @Input() changeField: string = 'delta';
  changeValue: string = '';

  @Input() mapSettings: any;
  // @Input() mapMetrics: Metrics;
  // @Input() mapRanges: number[][] = [];
  // @Input() legendColors: string[] = ["green","yellow","orange","redorange","red"];
  // @Input() legendLabels: string[] = ["trace 1","trace 2","trace 3","trace 4","trace 5"];
  markers: any;

  corridors: any;
  data: any;
  filteredData: any;

  constructor(private _filterService: FilterService,
    private _metricsService: MetricsService,
    private _formatService: FormatService) { }

    ngOnInit(): void {
      this._metricsService.getMetrics(this.graphMetrics).subscribe(response => {
        this.data = response;
        this._loadData();
      });

      this._filterSubscription = this._filterService.filters.subscribe(() => {
        if(this.data !== undefined){
          this._loadData();
        }
      });
    }

    ngOnDestroy(): void {
      this._filterSubscription.unsubscribe();
    }

    private _loadData(){
      if(this.data !== undefined){
        this.filteredData = this._filterService.filterData(this.data);
        this.corridors = new Set(this.filteredData.filter(value => value['corridor'] !== null).map(data => data['corridor']));

        let metricData = this._filterService.getZoneGroupData(this.filteredData);
        if(metricData !== undefined){
          if(this.graphMetrics.formatType === "percent"){
            this.metricValue = this._formatService.formatPercent(metricData[this.metricField], this.graphMetrics.formatDecimals);
          }else{
            this.metricValue = this._formatService.formatNumber(metricData[this.metricField], this.graphMetrics.formatDecimals);
          }

          this.changeValue = this._formatService.formatPercent(metricData[this.changeField],2);
        }
      }
    }
}
