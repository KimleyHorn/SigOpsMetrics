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
  tpMapLabels: string[] = ["0-5,000","5,000-10,000","10,000-15,000","15,000-20,000","Over 20,000"]
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

  //aog inputs
  aogdMetricField: string = 'aog';
  // aoghMetricField: string ='aogh';
  aogdGraphMetrics: Metrics = new Metrics();
  aogdMapMetrics: Metrics = new Metrics();
  aoghGraphMetrics: Metrics = new Metrics();
  aoghMapMetrics: Metrics = new Metrics();
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

  prdMetricField: string = 'pr';
  prdGraphMetrics: Metrics = new Metrics();
  prdMapMetrics: Metrics = new Metrics();
  prdMapLabels: string[] = ["0-1","1-2","2-3","3-4","Over 4"];
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
  qsdMetricField: string = 'qs_freq';
  qsdGraphMetrics: Metrics = new Metrics();
  qsdMapMetrics: Metrics = new Metrics();
  qsdMapLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","Over 0.8"];
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
  sfMetricField: string = 'sf_freq';
  sfGraphMetrics: Metrics = new Metrics();
  sfMapMetrics: Metrics = new Metrics();
  sfMapLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","Over 0.8"];
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

  constructor(private toggleService: ChartToggleService) {
    let mapStart = '2021-03-01';
    let mapEnd = '2021-03-02';

    this.tpGraphMetrics.measure = 'tp';
    this.tpMapMetrics.measure = 'tp';
    this.tpMapMetrics.level = 'sig';
    this.tpMapMetrics.start = mapStart;
    this.tpMapMetrics.end = mapEnd;

    this.aogdGraphMetrics.measure = 'aogd';
    this.aogdMapMetrics.measure = 'aogd';
    this.aogdMapMetrics.level = 'cor';
    this.aogdMapMetrics.start = mapStart;
    this.aogdMapMetrics.end = mapEnd;

    this.aoghGraphMetrics.measure = 'aogh';
    this.aoghGraphMetrics.start = mapStart;
    this.aoghGraphMetrics.end = mapEnd;

    this.prdGraphMetrics.measure = 'prd';
    this.prdMapMetrics.measure = 'prd';
    this.prdMapMetrics.level = 'sig';
    this.prdMapMetrics.start = mapStart;
    this.prdMapMetrics.end = mapEnd;

    this.qsdGraphMetrics.measure = 'qsd';
    this.qsdMapMetrics.measure = 'qsd';
    this.qsdMapMetrics.level = 'sig';
    this.qsdMapMetrics.start = mapStart;
    this.qsdMapMetrics.end = mapEnd;

    this.sfGraphMetrics.measure = 'sfo';
    this.sfMapMetrics.measure = 'sfo';
    this.sfMapMetrics.level = 'sig';
    this.sfMapMetrics.start = mapStart;
    this.sfMapMetrics.end = mapEnd;

    this.ttiGraphMetrics.measure = 'tti';

    this.ptiGraphMetrics.measure = 'pti';
   }

  ngOnInit(): void {
    this.toggleService.toggleValue.subscribe(value => {
      this.toggleValue = value;
    });
  }

}
