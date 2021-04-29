import { Component, ContentChild, Input, OnInit, TemplateRef } from '@angular/core';
import { Metrics } from 'src/app/models/metrics';
import { SignalInfo } from 'src/app/models/signal-info';
import { FilterService } from 'src/app/services/filter.service';
import { FormatService } from 'src/app/services/format.service';
import { MetricsService } from 'src/app/services/metrics.service';

@Component({
  selector: 'app-graph-dashboard',
  templateUrl: './graph-dashboard.component.html',
  styleUrls: ['./graph-dashboard.component.css']
})
export class GraphDashboardComponent implements OnInit {
  @ContentChild(TemplateRef) template: TemplateRef<any>;

  @Input() graphMetrics: Metrics;
  @Input() metricLabel: string = '';
  @Input() metricField: string = '';
  @Input() metricDecimals: number = 0;
  metricValue: string = '';

  @Input() changeLabel: string = '';
  @Input() changeField: string = 'delta';
  changeValue: string = '';

  @Input() mapMetrics: Metrics;
  @Input() mapColors: string[] = ["green","yellow","orange","redorange","red"];
  @Input() mapLabels: string[] = ["trace 1","trace 2","trace 3","trace 4","trace 5"];
  markers: any;

  corridors: any;
  data: any;
  filteredData: any;
  signals: SignalInfo[];

  constructor(private _filterService: FilterService,
    private _metricsService: MetricsService,
    private _formatService: FormatService) { }

    ngOnInit(): void {
      this.signals = [];
      this._metricsService.getMetrics(this.graphMetrics).subscribe(response => {
        this.data = response;
        this._loadData();
      });

      this._filterService.filters.subscribe(() => {
        if(this.data !== undefined){
          this._loadData();
        }
      });
    }

    private _loadData(){
      if(this.data !== undefined && this.signals !== undefined){
        this.filteredData = this._filterService.filterData(this.data);
        this.corridors = new Set(this.filteredData.filter(value => value['corridor'] !== null).map(data => data['corridor']));

        let metricData = this._filterService.getZoneGroupData(this.filteredData);

        if(metricData !== undefined){
          if(this.metricField === 'aog'){
            this.metricValue = this._formatService.formatPercent(metricData[this.metricField],1);
          }
          else{
            this.metricValue = this._formatService.formatNumber(metricData[this.metricField], this.metricDecimals);
          }
          this.changeValue = this._formatService.formatPercent(metricData[this.changeField],2);
        }
      }
    }
}
