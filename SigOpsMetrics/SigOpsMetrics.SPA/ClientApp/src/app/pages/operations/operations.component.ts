import { Component, OnInit } from '@angular/core';
import { ChartToggleService } from 'src/app/components/toggles/chart-toggle/chart-toggle.service';
import { Graph } from 'src/app/models/graph';
import { Metrics } from 'src/app/models/metrics';

@Component({
  selector: 'app-operations',
  templateUrl: './operations.component.html',
  styleUrls: ['./operations.component.css']
})
export class OperationsComponent implements OnInit {
  toggleValue: string;

  tpMetricField: string = 'vph';
  tpGraphMetrics: Metrics = new Metrics();
  tpMapMetrics: Metrics = new Metrics();
  tpTitle: string = 'Throughput (peak veh/hr)';
  tpBar: Graph = {
    title: 'Throughput (vph)',
    x: 'vph',
    y: 'corridor',
    hoverTemplate: 
      '<b>%{y}</b>' +
      '<br>Throughput (peak veh/hr): <b>%{x}</b>' +
      '<extra></extra>',
  };
  tpLine: Graph = {
    title: 'Vehicles per Hour Trend',
    x: 'month',
    y: 'vph',
    text: 'corridor',
    hoverTemplate: 
      '<b>%{text}</b>' +
      '<br>Week of: <b>%{x}</b>' +
      '<br>Throughput (peak veh/hr): <b>%{y}</b>' +
      '<extra></extra>'
  };

  //queue spillback rate inputs
  qsdMetricField: string = 'qs_freq';
  qsdGraphMetrics: Metrics = new Metrics();
  qsdMapMetrics: Metrics = new Metrics();
  qsdTitle: string = 'Queue Spillback Rate';
  qsdBar: Graph = {
    title: 'Queue Spillback Rate',
    x: 'qs_freq',
    y: 'corridor',
    hoverTemplate: 
      '<b>%{y}</b>' +
      '<br>Queue Spillback Rate: <b>%{x}</b>' +
      '<extra></extra>',
  };
  qsdLine: Graph = {
    title: 'Queue Spillback Trend',
    x: 'month',
    y: 'qs_freq',
    text: 'corridor',
    hoverTemplate: 
      '<b>%{text}</b>' +
      '<br>Week of: <b>%{x}</b>' +
      '<br>Queue Spillback Rate: <b>%{y}</b>' +
      '<extra></extra>'
  };

  constructor(private toggleService: ChartToggleService) {
    this.tpGraphMetrics.measure = 'tp';

    this.tpMapMetrics.measure = 'tp';
    this.tpMapMetrics.level = 'sig';
    this.tpMapMetrics.start = '2021-03-01';
    this.tpMapMetrics.end = '2021-03-02';

    this.qsdGraphMetrics.measure = 'qsd';

    this.qsdMapMetrics.measure = 'qsd';
    this.qsdMapMetrics.level = 'sig';
    this.qsdMapMetrics.start = '2021-03-01';
    this.qsdMapMetrics.end = '2021-03-02';
   }

  ngOnInit(): void {
    this.toggleService.toggleValue.subscribe(value => {
      this.toggleValue = value;
    });
  }

}
