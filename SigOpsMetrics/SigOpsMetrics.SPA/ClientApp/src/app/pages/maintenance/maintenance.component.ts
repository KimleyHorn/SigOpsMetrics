import { Component, OnInit } from '@angular/core';
import { ChartToggleService } from 'src/app/components/toggles/chart-toggle/chart-toggle.service';
import { Graph } from 'src/app/models/graph';
import { MapSettings } from 'src/app/models/map-settings';
import { Metrics } from 'src/app/models/metrics';

@Component({
  selector: 'app-maintenance',
  templateUrl: './maintenance.component.html',
  styleUrls: ['./maintenance.component.css'],
  providers: [MapSettings]
})
export class MaintenanceComponent implements OnInit {
  toggleValue: string;
  mapStart = '2021-03-01';
  mapEnd = '2021-03-02';

  dtvGraphMetrics: Metrics = new Metrics({ measure: "vpd" });
  dtvTitle: string = 'Traffic Volume [veh/day]';
  dtvBar: Graph = {
    title: 'Selected Month',
    x: 'vpd',
    y: 'corridor',
    hoverTemplate:
      '<b>%{y}</b>' +
      '<br>Traffic Volume [veh/day]: <b>%{x}</b>' +
      '<extra></extra>',
  };
  dtvLine: Graph = {
    title: 'Weekly Trend',
    x: 'month',
    y: 'vpd',
    text: 'corridor',
    hoverTemplate:
      '<b>%{text}</b>' +
      '<br>Week of: <b>%{x}</b>' +
      '<br>Traffic Volume [veh/day]: <b>%{y}</b>' +
      '<extra></extra>'
  };

  papdGraphMetrics: Metrics = new Metrics({ measure: "papd" });
  papdTitle: string = 'Pedestrian Activations per Day [pa/day]';
  papdBar: Graph = {
    title: 'Selected Month',
    x: 'papd',
    y: 'corridor',
    hoverTemplate:
      '<b>%{y}</b>' +
      '<br>Pedestrian Activations per Day [pa/day]: <b>%{x}</b>' +
      '<extra></extra>',
  };
  papdLine: Graph = {
    title: 'Weekly Trend',
    x: 'month',
    y: 'papd',
    text: 'corridor',
    hoverTemplate:
      '<b>%{text}</b>' +
      '<br>Week of: <b>%{x}</b>' +
      '<br>Pedestrian Activations per Day [pa/day]: <b>%{y}</b>' +
      '<extra></extra>'
  };
  duGraphMetrics: Metrics = new Metrics({ measure: "du", formatDecimals: 1, formatType: "percent" });
  duTitle: string = 'Detector Uptime [%]';
  duBar: Graph = {
    title: 'Selected Month',
    x: 'uptime',
    y: 'corridor',
    hoverTemplate:
      '<b>%{y}</b>' +
      '<brUptime: <b>%{x}</b>' +
      '<extra></extra>',
  };
  duLine: Graph = {
    title: 'Weekly Trend',
    x: 'month',
    y: 'uptime',
    text: 'corridor',
    hoverTemplate:
      '<b>%{text}</b>' +
      '<br>Week of: <b>%{x}</b>' +
      '<br>Uptime: <b>%{y}</b>' +
      '<extra></extra>'
  };
  pauGraphMetrics: Metrics = new Metrics({ measure: "pau", formatDecimals: 1, formatType: "percent" });
  pauTitle: string = 'Pedestrian Pushbutton Uptime [%]';
  pauBar: Graph = {
    title: 'Selected Month',
    x: 'uptime',
    y: 'corridor',
    hoverTemplate:
      '<b>%{y}</b>' +
      '<br>Pedestrian Pushbutton Uptime [%]: <b>%{x}</b>' +
      '<extra></extra>',
  };
  pauLine: Graph = {
    title: 'Weekly Trend',
    x: 'month',
    y: 'uptime',
    text: 'corridor',
    hoverTemplate:
      '<b>%{text}</b>' +
      '<br>Week of: <b>%{x}</b>' +
      '<br>Pedestrian Pushbutton Uptime [%]: <b>%{y}</b>' +
      '<extra></extra>'
  };
  cctvGraphMetrics: Metrics = new Metrics({ measure: "cctv", formatDecimals: 1, formatType: "percent" });
  cctvTitle: string = 'CCTV Uptime [%]';
  cctvBar: Graph = {
    title: 'Selected Month',
    x: 'uptime',
    y: 'corridor',
    hoverTemplate:
      '<b>%{y}</b>' +
      '<br>CCTV Uptime [%]: <b>%{x}</b>' +
      '<extra></extra>',
  };
  cctvLine: Graph = {
    title: 'Weekly Trend',
    x: 'month',
    y: 'uptime',
    text: 'corridor',
    hoverTemplate:
      '<b>%{text}</b>' +
      '<br>Week of: <b>%{x}</b>' +
      '<br>CCTV Uptime [%]: <b>%{y}</b>' +
      '<extra></extra>'
  };
  cuGraphMetrics: Metrics = new Metrics({ measure: "cu", formatDecimals: 1, formatType: "percent" });
  cuTitle: string = 'Communication Uptime [%]';
  cuBar: Graph = {
    title: 'Selected Month',
    x: 'uptime',
    y: 'corridor',
    hoverTemplate:
      '<b>%{y}</b>' +
      '<br>Communication Uptime [%]: <b>%{x}</b>' +
      '<extra></extra>',
  };
  cuLine: Graph = {
    title: 'Weekly Trend',
    x: 'month',
    y: 'uptime',
    text: 'corridor',
    hoverTemplate:
      '<b>%{text}</b>' +
      '<br>Week of: <b>%{x}</b>' +
      '<br>Communication Uptime [%]: <b>%{y}</b>' +
      '<extra></extra>'
  };

  constructor(private toggleService: ChartToggleService, public mapSettings: MapSettings) { }

  ngOnInit(): void {
    this.toggleService.toggleValue.subscribe(value => {
      this.toggleValue = value;
    });
  }

}
