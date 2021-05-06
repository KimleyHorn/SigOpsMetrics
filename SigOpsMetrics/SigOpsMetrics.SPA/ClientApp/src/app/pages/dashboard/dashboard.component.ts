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
  legendLabels: string[];
  legendColors: string[];
  mapRanges: number[][];

  constructor(public mapSettings: MapSettings, private _metricSelectService: MetricSelectService) { }

  ngOnInit(){
    this.mapField = this.mapSettings.tpMapField;
    this.mapMetrics = this.mapSettings.tpMapMetrics;
    this.legendLabels = this.mapSettings.tpLegendLabels;
    this.legendColors = this.mapSettings.tpLegendColors;
    this.mapRanges = this.mapSettings.tpMapRanges;

    this._metricSelectService.selectedMetric.subscribe(value => {
      switch (value) {
        case "aogd":
          this.mapField = this.mapSettings.aogdMapField;
          this.mapMetrics = this.mapSettings.aogdMapMetrics;
          this.legendLabels = this.mapSettings.aogdLegendLabels;
          this.legendColors = this.mapSettings.aogdLegendColors;
          this.mapRanges = this.mapSettings.aogdMapRanges;
          break;
        case "prd":
          this.mapField = this.mapSettings.prdMapField;
          this.mapMetrics = this.mapSettings.prdMapMetrics;
          this.legendLabels = this.mapSettings.prdLegendLabels;
          this.legendColors = this.mapSettings.prdLegendColors;
          this.mapRanges = this.mapSettings.prdMapRanges;
          break;
        case "qsd":
          this.mapField = this.mapSettings.qsdMapField;
          this.mapMetrics = this.mapSettings.qsdMapMetrics;
          this.legendLabels = this.mapSettings.qsdLegendLabels;
          this.legendColors = this.mapSettings.qsdLegendColors;
          this.mapRanges = this.mapSettings.qsdMapRanges;
          break;
        case "sfd":
          this.mapField = this.mapSettings.psfMapField;
          this.mapMetrics = this.mapSettings.psfMapMetrics;
          this.legendLabels = this.mapSettings.psfLegendLabels;
          this.legendColors = this.mapSettings.psfLegendColors;
          this.mapRanges = this.mapSettings.psfMapRanges;
          break;
        case "sfo":
          this.mapField = this.mapSettings.osfMapField;
          this.mapMetrics = this.mapSettings.osfMapMetrics;
          this.legendLabels = this.mapSettings.osfLegendLabels;
          this.legendColors = this.mapSettings.osfLegendColors;
          this.mapRanges = this.mapSettings.osfMapRanges;
          break;
        default:
          this.mapField = this.mapSettings.tpMapField;
          this.mapMetrics = this.mapSettings.tpMapMetrics;
          this.legendLabels = this.mapSettings.tpLegendLabels;
          this.legendColors = this.mapSettings.tpLegendColors;
          this.mapRanges = this.mapSettings.tpMapRanges;
          break;
      }
    });
  }
}
