import { Component, EventEmitter, OnInit, Output } from "@angular/core";
import { MatTabChangeEvent } from "@angular/material/tabs";
import { Title } from "@angular/platform-browser";
import { ChartToggleService } from "src/app/components/toggles/chart-toggle/chart-toggle.service";
import { Graph } from "src/app/models/graph";
import { MapSettings } from "src/app/models/map-settings";
import { Metrics } from "src/app/models/metrics";
import { FilterService } from "src/app/services/filter.service";

@Component({
  selector: "app-operations",
  templateUrl: "./operations.component.html",
  styleUrls: ["./operations.component.css"],
  providers: [MapSettings],
})
export class OperationsComponent implements OnInit {
  @Output("resetErrorState") resetErrorState: EventEmitter<any> =
    new EventEmitter();
  toggleValue: string;
  currentTab: string;

  tpGraphMetrics: Metrics = new Metrics({ measure: "tp" });
  tpTitle: string = "Throughput (peak veh/hr)";
  tpBar: Graph = {
    title: "Throughput (vph)",
    x: "vph",
    y: "corridor",
    hoverTemplate:
      "<b>%{y}</b>" +
      "<br>Throughput (peak veh/hr): <b>%{x}</b>" +
      "<extra></extra>",
  };
  tpLine: Graph = {
    title: "Vehicles per Hour Trend",
    x: "month",
    y: "vph",
    text: "corridor",
    hoverTemplate:
      "<b>%{text}</b>" +
      "<br><b>%{x}</b>" +
      "<br>Throughput (peak veh/hr): <b>%{y}</b>" +
      "<extra></extra>",
  };

  dtvGraphMetrics: Metrics = new Metrics({ measure: "vpd" });
  dtvTitle: string = "Traffic Volume [veh/day]";
  dtvBar: Graph = {
    title: "Selected Month",
    x: "vpd",
    y: "corridor",
    hoverTemplate:
      "<b>%{y}</b>" +
      "<br>Traffic Volume [veh/day]: <b>%{x}</b>" +
      "<extra></extra>",
  };
  dtvLine: Graph = {
    title: "Weekly Trend",
    x: "month",
    y: "vpd",
    text: "corridor",
    hoverTemplate:
      "<b>%{text}</b>" +
      "<br><b>%{x}</b>" +
      "<br>Traffic Volume [veh/day]: <b>%{y}</b>" +
      "<extra></extra>",
  };

  aogdGraphMetrics: Metrics = new Metrics({
    measure: "aogd",
    formatDecimals: 1,
    formatType: "percent",
  });
  aoghGraphMetrics: Metrics = new Metrics({
    measure: "aogh",
    formatDecimals: 1,
    formatType: "percent",
  });
  aogTitle: string = "Arrivals on Green [%]";
  aogBar: Graph = {
    title: "Arrivals on Green",
    x: "aog",
    y: "corridor",
    hoverTemplate:
      "<b>%{y}</b>" + "<br>Selected Month <b>%{x}</b>" + "<extra></extra>",
  };
  aogdLine: Graph = {
    title: "Weekly Trend",
    x: "month",
    y: "aog",
    text: "corridor",
    hoverTemplate:
      "<b>%{text}</b>" +
      "<br><b>%{x}</b>" +
      "<br>Arrivals on Green: <b>%{y}</b>" +
      "<extra></extra>",
  };
  aoghLine: Graph = {
    title: "Arrivals on Green [%]",
    x: "hour",
    y: "aog",
    text: "corridor",
    hoverTemplate:
      "<b>%{text}</b>" +
      "<br><b>%{x}</b>" +
      "<br>Arrivals on Green: <b>%{y}</b>" +
      "<extra></extra>",
  };

  prdGraphMetrics: Metrics = new Metrics({ measure: "prd", formatDecimals: 2 });
  prdTitle: string = "Progression Ratio";
  prdBar: Graph = {
    title: "Selected Month",
    x: "pr",
    y: "corridor",
    hoverTemplate:
      "<b>%{y}</b>" + "<br>Progression Ratio: <b>%{x}</b>" + "<extra></extra>",
  };
  prdLine: Graph = {
    title: "Weekly Trend",
    x: "month",
    y: "pr",
    text: "corridor",
    hoverTemplate:
      "<b>%{text}</b>" +
      "<br><b>%{x}</b>" +
      "<br>Progression Ratio: <b>%{x}</b>" +
      "<extra></extra>",
  };

  //queue spillback rate inputs
  qsdGraphMetrics: Metrics = new Metrics({
    measure: "qsd",
    formatDecimals: 1,
    formatType: "percent",
  });
  qsdTitle: string = "Queue Spillback Rate";
  qsdBar: Graph = {
    title: "Queue Spillback Rate",
    x: "qs_freq",
    y: "corridor",
    hoverTemplate:
      "<b>%{y}</b>" +
      "<br>Queue Spillback Rate: <b>%{x}</b>" +
      "<extra></extra>",
  };
  qsdLine: Graph = {
    title: "Queue Spillback Trend",
    x: "month",
    y: "qs_freq",
    text: "corridor",
    hoverTemplate:
      "<b>%{text}</b>" +
      "<br><b>%{x}</b>" +
      "<br>Queue Spillback Rate: <b>%{y}</b>" +
      "<extra></extra>",
  };

  //split failure rate inputs
  psfGraphMetrics: Metrics = new Metrics({
    measure: "sfd",
    formatDecimals: 2,
    formatType: "percent",
  });
  osfGraphMetrics: Metrics = new Metrics({
    measure: "sfo",
    formatDecimals: 2,
    formatType: "percent",
  });
  sfTitle: string = "Split Failures Rate [%]";
  sfBar: Graph = {
    title: "Selected Month",
    x: "sf_freq",
    y: "corridor",
    hoverTemplate:
      "<b>%{y}</b>" +
      "<br>Split Failures Rate[%]: <b>%{x}</b>" +
      "<extra></extra>",
  };
  sfLine: Graph = {
    title: "Weekly Trend",
    x: "month",
    y: "sf_freq",
    text: "corridor",
    hoverTemplate:
      "<b>%{text}</b>" +
      "<br><b>%{x}</b>" +
      "<br>Split Failures Rate[%]: <b>%{y}</b>" +
      "<extra></extra>",
  };

  //travel time index inputs
  ttiMetricField: string = "tti";
  ttiGraphMetrics: Metrics = new Metrics({ measure: "tti", formatDecimals: 2 });
  ttiTitle: string = "Travel Time Index (TTI)";
  ttiBar: Graph = {
    title: "Selected Month TTI",
    x: "tti",
    y: "corridor",
    hoverTemplate:
      "<b>%{y}</b>" + "<br>Travel Time Index: <b>%{y}</b>" + "<extra></extra>",
  };
  ttiLine: Graph = {
    title: "Monthly Trend",
    x: "month",
    y: "tti",
    text: "corridor",
    hoverTemplate:
      "<b>%{text}</b>" +
      "<br><b>%{x}</b>" +
      "<br>Travel Time Index: <b>%{y}</b>" +
      "<extra></extra>",
  };

  //planning time index inputs
  ptiMetricField: string = "pti";
  ptiGraphMetrics: Metrics = new Metrics({ measure: "pti", formatDecimals: 2 });
  ptiTitle: string = "Planning Time Index (PTI)";
  ptiBar: Graph = {
    title: "Selected Month PTI",
    x: "pti",
    y: "corridor",
    hoverTemplate:
      "<b>%{y}</b>" +
      "<br>Planning Time Index: <b>%{y}</b>" +
      "<extra></extra>",
  };
  ptiLine: Graph = {
    title: "Monthly Trend",
    x: "month",
    y: "pti",
    text: "corridor",
    hoverTemplate:
      "<b>%{text}</b>" +
      "<br><b>%{x}</b>" +
      "<br>Planning Time Index: <b>%{y}</b>" +
      "<extra></extra>",
  };

  constructor(
    private toggleService: ChartToggleService,
    public mapSettings: MapSettings,
    private titleService: Title,
    private filterService: FilterService
  ) {}

  ngOnInit(): void {
    this.toggleService.toggleValue.subscribe((value) => {
      this.toggleValue = value;
    });
    this.currentTab = "Throughput";

    this.titleService.setTitle("SigOpsMetrics - Operations - Throughput");
    this.filterService.updateFilterErrorState(1);
  }

  tabChanged(tabChangeEvent: MatTabChangeEvent): void {
    this.currentTab = tabChangeEvent.tab.textLabel;
    this.titleService.setTitle(
      "SigOpsMetrics - Operations - " + this.currentTab
    );
    this.filterService.updateFilterErrorState(1);
  }
}
