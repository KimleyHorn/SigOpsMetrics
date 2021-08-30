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

  constructor(private _filterService: FilterService,
    private _metricsService: MetricsService,
    private _formatService: FormatService) { }

    ngOnInit(): void {
      this._filterSubscription = this._filterService.filters.subscribe(filter => {
        this.filterState = filter;
        this._metricsSubscription = this._metricsService.filterMetrics(this.graphMetrics, filter).subscribe(response => {
          if (this.filterState.zone_Group === 'All') {
            let allRegions = ['Cobb County','District 1','District 2','District 3','District 4','District 5','District 6','District 7','Ramp Meters','RTOP1','RTOP2']
            let _data = [];
            console.log(response);
            response.forEach(x => {
              if (allRegions.includes(x.zone_Group)) {
                let found = false;
                for (let i=0; i<_data.length;i++) {
                  if (_data[i].month == x.month && _data[i].zone_Group == x.zone_Group) {
                    _data[i].vph += x.vph;
                    _data[i].delta += x.delta; //is this an avg?
                    found = true;
                    break;
                  }
                }
                if (!found) {
                  x.corridor = x.zone_Group;
                  x.description = x.zone_Group;
                  _data.push(x);
                }
              }
            })
            this.data = _data;
            console.log(this.data);
          } else {
            this.data = response;
          }
          this._loadData();
          this._metricsSubscription.unsubscribe();
        });

        this._averageSubscription = this._metricsService.averageMetrics(this.graphMetrics, filter).subscribe(response => {
          this.averageData = response;
          this._loadData();
          this._averageSubscription.unsubscribe();
        })
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
        let metricData = this._filterService.getAverageData(this.averageData);

        if(metricData !== undefined){
          if(this.graphMetrics.formatType === "percent"){
            this.metricValue = this._formatService.formatPercent(metricData.avg, this.graphMetrics.formatDecimals);
          }else{
            this.metricValue = this._formatService.formatNumber(metricData.avg, this.graphMetrics.formatDecimals);
          }

          this.changeValue = this._formatService.formatPercent(metricData.delta,2);
        }
      }
    }
}
