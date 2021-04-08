import { Component, ContentChild, Input, OnInit, TemplateRef } from '@angular/core';
import { Graph } from 'src/app/models/graph';
import { Metrics } from 'src/app/models/metrics';
import { SignalInfo } from 'src/app/models/signal-info';
import { FilterService } from 'src/app/services/filter.service';
import { FormatService } from 'src/app/services/format.service';
import { MetricsService } from 'src/app/services/metrics.service';
import { SignalsService } from 'src/app/services/signals.service';

@Component({
  selector: 'app-graph-dashboard',
  templateUrl: './graph-dashboard.component.html',
  styleUrls: ['./graph-dashboard.component.css']
})
export class GraphDashboardComponent implements OnInit {
  @ContentChild(TemplateRef) template: TemplateRef<any>;

  @Input() metrics: Metrics;
  @Input() metricLabel: string = '';
  @Input() metricField: string = '';
  metricValue: string = '';
  
  @Input() changeLabel: string = '';
  @Input() changeField: string = 'delta';
  changeValue: string = '';

  corridors: any;
  data: any;
  filteredData: any;
  signals: SignalInfo[];

  constructor(private _filterService: FilterService, 
    private _metricsService: MetricsService,
    private _formatService: FormatService,
    private _signalsService: SignalsService) { }

    ngOnInit(): void {
      this._signalsService.getData().subscribe(response => {
        this.signals = response;

        this._loadData();
      });

      this._metricsService.getMetrics(this.metrics).subscribe(response => {
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
      this.filteredData = this._filterService.filterData(this.data);
      this.corridors = new Set(this.filteredData.filter(value => value['corridor'] !== null).map(data => data['corridor']));

      if(this.signals !== undefined){
        console.log(this.signals);
        console.log(this.data);
      }

      let metricData = this._filterService.getZoneGroupData(this.filteredData);
      if(metricData !== undefined){
        this.metricValue = this._formatService.formatNumber(metricData[this.metricField]);
        this.changeValue = this._formatService.formatPercent(metricData[this.changeField],2);
      }
    }
}
