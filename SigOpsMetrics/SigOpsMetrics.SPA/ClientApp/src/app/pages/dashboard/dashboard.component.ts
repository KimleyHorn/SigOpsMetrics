import { Component, OnInit } from '@angular/core';
import { setUncaughtExceptionCaptureCallback } from 'process';
import { MetricSelectService } from 'src/app/components/selects/metric-select/metric-select.service';
import { MapSettings } from 'src/app/models/map-settings';
import { Metrics } from 'src/app/models/metrics';
import { BrowserModule, Title } from '@angular/platform-browser';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
  providers:[MapSettings]
})


export class DashboardComponent  implements OnInit {
  selectedSettings;

  performanceTableData = [
    { name: "Throughput", measure: "tp", metric: "vph", format:"number", precision: 0},
    { name: "Arrivals on Green", measure: "aogd", metric: "aog", format:"percent", precision: 1 },
    { name: "Progression Rate", measure: "prd", metric: "pr", format:"number", precision: 2 },
    { name: "Spillback Rate", measure: "qsd", metric: "qs_freq", format:"percent", precision: 1 },
    { name: "Peak Period Split Failures", measure: "sfd", metric: "sf_freq", format:"percent", precision:  1, value:0 },
    { name: "Off-Peak Split Failures", measure: "sfo", metric: "sf_freq", format:"percent", precision: 1, value: 0 },
    { name: "Travel Time Index", measure: "tti", metric: "tti", format:"number", precision: 2 },
    { name: "Planning Time Index", measure: "pti", metric: "pti", format:"number", precision: 2 },
  ];

  volumeTableData = [
    { name: "Traffic Volume", measure: "vpd", metric: "vpd", format:"number", precision: 0},
    { name: "AM Peak Volume", measure: "vphpa", metric: "vph", format:"number", precision: 0 },
    { name: "PM Peak Volume", measure: "vphpp", metric: "vph", format:"number", precision: 0 },
    { name: "Pedestrian Activitations", measure: "papd", metric: "papd", format:"number", precision: 0 },
    { name: "Vehicle Detector Uptime", measure: "du", metric: "uptime", format:"percent", precision: 1 },
    { name: "Pedestrian Pushbutton Uptime", measure: "pau", metric: "uptime", format:"percent", precision: 1 },
    { name: "CCTV Uptime", measure: "cctv", metric: "uptime", format:"percent", precision: 1 },
    { name: "Communications Uptime", measure: "cu", metric: "uptime", format:"percent", precision: 1 },
  ];

  constructor(public mapSettings: MapSettings, private _metricSelectService: MetricSelectService, private titleService:Title) { }

  ngOnInit(){
    this.titleService.setTitle("SigOpsMetrics - Dashboard")
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
          this.selectedSettings = this.mapSettings.papdMapSettings;
          break;
          case "du":
          this.selectedSettings = this.mapSettings.duMapSettings;
          break;
          case "pau":
          this.selectedSettings = this.mapSettings.pauMapSettings;
          break;
          case "cctv":
          this.selectedSettings = this.mapSettings.cctvMapSettings;
          break;
          case "cu":
          this.selectedSettings = this.mapSettings.cuMapSettings;
          break;
        default:
          this.selectedSettings = this.mapSettings.tpMapSettings;
          break;
      }
    });
  }
}
