import { Component, OnInit } from '@angular/core';
import { ChartToggleService } from 'src/app/components/toggles/chart-toggle/chart-toggle.service';
import { Graph } from 'src/app/models/graph';
import { MapSettings } from 'src/app/models/map-settings';
import { Metrics } from 'src/app/models/metrics';

@Component({
  selector: 'app-health-metrics',
  templateUrl: './health-metrics.component.html',
  styleUrls: ['./health-metrics.component.css'],
  providers: [MapSettings]
})



export class HealthMetricsComponent implements OnInit {
  toggleValue: string;

  mtGraphMetrics: Metrics = new Metrics({ measure: "maint_plot", formatDecimals: 1  });
  mtTitle: string = 'Percent Health';
  mtBar: Graph = {
    title: 'Selected Month',
    x: 'percent Health',
    y: 'corridor',
    hoverTemplate:
      '<b>%{y}</b>' +
      '<br>Percent Health: <b>%{x}</b>' +
      '<extra></extra>',
  };
  mtLine: Graph = {
    title: 'Weekly Trend',
    x: 'month',
    y: 'percent Health',
    text: 'corridor',
    hoverTemplate:
      '<b>%{text}</b>' +
      '<br>Week of: <b>%{x}</b>' +
      '<br>Percent Health: <b>%{y}</b>' +
      '<extra></extra>'
  };

  
  otGraphMetrics: Metrics = new Metrics({ measure: "ops_plot", formatDecimals: 1, formatType: "percent"  });
  otTitle: string = 'Percent Health';
  otBar: Graph = {
    title: 'Selected Month',
    x: 'pr',
    y: 'corridor',
    hoverTemplate:
      '<b>%{y}</b>' +
      '<br>Progression Ratio: <b>%{x}</b>' +
      '<extra></extra>',
  };
  otLine: Graph = {
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

  stGraphMetrics: Metrics = new Metrics({ measure: "safety_plot", formatDecimals: 1, formatType: "percent" });
  stTitle: string = 'Percent Health';
  stBar: Graph = {
    title: 'Selected Month',
    x: 'pr',
    y: 'corridor',
    hoverTemplate:
      '<b>%{y}</b>' +
      '<br>Progression Ratio: <b>%{x}</b>' +
      '<extra></extra>',
  };
  stLine: Graph = {
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

  constructor(private toggleService: ChartToggleService, public mapSettings: MapSettings) {}

  ngOnInit(): void {
    this.toggleService.toggleValue.subscribe(value => {
      this.toggleValue = value;
    });
  }

}
