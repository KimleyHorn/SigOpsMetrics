import { Component, OnInit } from '@angular/core';
import { MetricSelectService } from 'src/app/components/selects/metric-select/metric-select.service';
import { MapSettings } from 'src/app/models/map-settings';
import { Metrics } from 'src/app/models/metrics';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
  providers:[MapSettings]
})
export class DashboardComponent  implements OnInit {
  mapField: string;
  mapMetrics: Metrics;
  mapLabels: string[];
  mapColors: string[];

  constructor(public mapSettings: MapSettings, private _metricSelectService: MetricSelectService) { }

  ngOnInit(){
    this.mapField = this.mapSettings.tpMapField;
    this.mapMetrics = this.mapSettings.tpMapMetrics;
    this.mapLabels = this.mapSettings.tpMapLabels;

    this._metricSelectService.selectedMetric.subscribe(value => {
      switch (value) {
        case "aogd":
          console.log(this.mapSettings.aogdMapMetrics);
          this.mapField = this.mapSettings.aogdMapField;
          this.mapMetrics = this.mapSettings.aogdMapMetrics;
          this.mapLabels = this.mapSettings.aogdMapLabels;
          break;
        case "prd":
          this.mapField = this.mapSettings.prdMapField;
          this.mapMetrics = this.mapSettings.prdMapMetrics;
          this.mapLabels = this.mapSettings.prdMapLabels;
          break;
        case "qsd":
          this.mapField = this.mapSettings.qsdMapField;
          this.mapMetrics = this.mapSettings.qsdMapMetrics;
          this.mapLabels = this.mapSettings.qsdMapLabels;
          break;
        default:
          this.mapField = this.mapSettings.tpMapField;
          this.mapMetrics = this.mapSettings.tpMapMetrics;
          this.mapLabels = this.mapSettings.tpMapLabels;
          break;
      }
    });
  }
}
