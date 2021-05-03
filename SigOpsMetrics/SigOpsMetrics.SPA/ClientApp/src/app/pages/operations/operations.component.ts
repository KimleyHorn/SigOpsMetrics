import { Component, OnInit } from '@angular/core';
import { ChartToggleService } from 'src/app/components/toggles/chart-toggle/chart-toggle.service';
import { Graph } from 'src/app/models/graph';
import { MapSettings } from 'src/app/models/map-settings';
import { Metrics } from 'src/app/models/metrics';

@Component({
  selector: 'app-operations',
  templateUrl: './operations.component.html',
  styleUrls: ['./operations.component.css'],
  providers: [MapSettings]
})
export class OperationsComponent implements OnInit {
  toggleValue: string;

  tpGraphMetrics: Metrics = new Metrics();
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

  aogdGraphMetrics: Metrics = new Metrics();
  aoghGraphMetrics: Metrics = new Metrics();
  aogTitle: string = 'Arrivals on Green [%]';
  aogBar: Graph = {
    title: 'Arrivals on Green',
    x: 'aog',
    y: 'corridor',
    hoverTemplate:
      '<b>%{y}</b>' +
      '<br>Selected Month <b>%{x}</b>' +
      '<extra></extra>',
  };
  aogdLine: Graph = {
    title: 'Weekly Trend',
    x: 'month',
    y: 'aog',
    text: 'corridor',
    hoverTemplate:
      '<b>%{text}</b>' +
      '<br>Week of: <b>%{x}</b>' +
      '<br>Arrivals on Green: <b>%{y}</b>' +
      '<extra></extra>'
  };
  aoghLine: Graph = {
    title: 'Arrivals on Green [%]',
    x: 'hour',
    y: 'aog',
    text: 'corridor',
    hoverTemplate:
      '<b>%{text}</b>' +
      '<br>Week of: <b>%{x}</b>' +
      '<br>Arrivals on Green: <b>%{y}</b>' +
      '<extra></extra>'
  };

  prdGraphMetrics: Metrics = new Metrics();
  prdTitle: string = 'Progression Ratio';
  prdBar: Graph = {
    title: 'Selected Month',
    x: 'pr',
    y: 'corridor',
    hoverTemplate:
      '<b>%{y}</b>' +
      '<br>Progression Ratio: <b>%{x}</b>' +
      '<extra></extra>',
  };
  prdLine: Graph = {
    title: 'Weekly Trend',
    x: 'month',
    y: 'pr',
    text: 'corridor',
    hoverTemplate:
      '<b>%{text}</b>' +
      '<br>Week of: <b>%{x}</b>' +
      '<br>Progression Ratio: <b>%{x}</b>' +
      '<extra></extra>'
  };

  //queue spillback rate inputs
  qsdGraphMetrics: Metrics = new Metrics();
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

  //split failure rate inputs
  psfGraphMetrics: Metrics = new Metrics();
  osfGraphMetrics: Metrics = new Metrics();
  sfTitle: string = 'Split Failures Rate [%]';
  sfBar: Graph = {
    title: 'Selected Month',
    x: 'sf_freq',
    y: 'corridor',
    hoverTemplate:
      '<b>%{y}</b>' +
      '<br>Split Failures Rate[%]: <b>%{y}</b>' +
      '<extra></extra>',
  };
  sfLine: Graph = {
    title: 'Weekly Trend',
    x: 'month',
    y: 'sf_freq',
    text: 'corridor',
    hoverTemplate:
      '<b>%{text}</b>' +
      '<br>Week of: <b>%{x}</b>' +
      '<br>Split Failures Rate[%]: <b>%{y}</b>' +
      '<extra></extra>'
  };

  //travel time index inputs
  ttiMetricField: string = 'tti';
  ttiGraphMetrics: Metrics = new Metrics();
  ttiTitle: string = 'Travel Time Index (TTI)';
  ttiBar: Graph = {
    title: 'Selected Month TTI',
    x: 'tti',
    y: 'corridor',
    hoverTemplate:
      '<b>%{y}</b>' +
      '<br>Travel Time Index: <b>%{y}</b>' +
      '<extra></extra>',
  };
  ttiLine: Graph = {
    title: 'Monthly Trend',
    x: 'month',
    y: 'tti',
    text: 'corridor',
    hoverTemplate:
      '<b>%{text}</b>' +
      '<br>Month of: <b>%{x}</b>' +
      '<br>Travel Time Index: <b>%{y}</b>' +
      '<extra></extra>'
  };

  //planning time index inputs
  ptiMetricField: string = 'pti';
  ptiGraphMetrics: Metrics = new Metrics();
  ptiTitle: string = 'Planning Time Index (PTI)';
  ptiBar: Graph = {
    title: 'Selected Month PTI',
    x: 'pti',
    y: 'corridor',
    hoverTemplate:
      '<b>%{y}</b>' +
      '<br>Planning Time Index: <b>%{y}</b>' +
      '<extra></extra>',
  };
  ptiLine: Graph = {
    title: 'Monthly Trend',
    x: 'month',
    y: 'pti',
    text: 'corridor',
    hoverTemplate:
      '<b>%{text}</b>' +
      '<br>Month of: <b>%{x}</b>' +
      '<br>Planning Time Index: <b>%{y}</b>' +
      '<extra></extra>'
  };

  constructor(private toggleService: ChartToggleService, public mapSettings: MapSettings) {
    let mapStart = '2021-03-01';
    let mapEnd = '2021-03-02';

    this.tpGraphMetrics.measure = 'tp';

    this.aogdGraphMetrics.measure = 'aogd';

    this.aoghGraphMetrics.measure = 'aogh';
    this.aoghGraphMetrics.start = mapStart;
    this.aoghGraphMetrics.end = mapEnd;

    this.prdGraphMetrics.measure = 'prd';

    this.qsdGraphMetrics.measure = 'qsd';

    this.psfGraphMetrics.measure = 'sfd';
    this.osfGraphMetrics.measure = 'sfo';

    this.ttiGraphMetrics.measure = 'tti';

    this.ptiGraphMetrics.measure = 'pti';
   }

  ngOnInit(): void {
    this.toggleService.toggleValue.subscribe(value => {
      this.toggleValue = value;
    });
  }

}
