import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { MetricSelectService } from './metric-select.service';

@Component({
  selector: 'app-metric-select',
  templateUrl: './metric-select.component.html',
  styleUrls: ['./metric-select.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class MetricSelectComponent implements OnInit {
  metrics: any[] = [
    { text: "Throughput", value: "tp" },
    { text: "Arrivals on Green", value: "aogd" },
    { text: "Progression Rate", value: "prd" },
    { text: "Spillback Rate", value: "qsd" },
    // { text: "Peak Period Split Failures", value: "" },
    // { text: "Off-Peak Split Failures", value: "" },
  ]

  selectedMetric: string = "tp";

  constructor(public metricSelectService: MetricSelectService) { }

  ngOnInit(): void {
  }
}
