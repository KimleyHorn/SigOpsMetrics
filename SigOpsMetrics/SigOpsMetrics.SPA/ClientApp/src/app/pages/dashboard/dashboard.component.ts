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
  selectedSettings;

  constructor(public mapSettings: MapSettings, private _metricSelectService: MetricSelectService) { }

  ngOnInit(){
    this.selectedSettings = this.mapSettings.tpMapSettings;

    this._metricSelectService.selectedMetric.subscribe(value => {
      switch (value) {
        case "aogd":
          this.selectedSettings = this.mapSettings.aogdMapSettings;
          break;
        case "prd":
          this.selectedSettings = this.mapSettings.prdMapSettings;
          break;
        case "qsd":
          this.selectedSettings = this.mapSettings.qsdMapSettings;
          break;
        case "sfd":
          this.selectedSettings = this.mapSettings.psfMapSettings;
          break;
        case "sfo":
          this.selectedSettings = this.mapSettings.osfMapSettings;
          break;
        case "vpd":
          this.selectedSettings = this.mapSettings.dtvMapSettings;
          break;
          case "papd":
          this.selectedSettings = this.mapSettings.dtvMapSettings;
          break;
          case "du":
          this.selectedSettings = this.mapSettings.dtvMapSettings;
          break;
          case "pau":
          this.selectedSettings = this.mapSettings.dtvMapSettings;
          break;
          case "cctv":
          this.selectedSettings = this.mapSettings.dtvMapSettings;
          break;
          case "cu":
          this.selectedSettings = this.mapSettings.dtvMapSettings;
          break;
        default:
          this.selectedSettings = this.mapSettings.tpMapSettings;
          break;
      }
    });
  }
}
