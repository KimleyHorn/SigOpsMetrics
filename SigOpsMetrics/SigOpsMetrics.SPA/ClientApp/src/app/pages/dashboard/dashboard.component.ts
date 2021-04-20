import { Component } from '@angular/core';
import { Metrics } from 'src/app/models/metrics';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent {

  tpMetricField: string = 'vph';
  tpGraphMetrics: Metrics = new Metrics();
  tpMapMetrics: Metrics = new Metrics();

  constructor() {
    this.tpGraphMetrics.measure = 'tp';

    this.tpMapMetrics.measure = 'tp';
    this.tpMapMetrics.level = 'sig';
    this.tpMapMetrics.start = '2021-03-01';
    this.tpMapMetrics.end = '2021-03-02';
  }
}
