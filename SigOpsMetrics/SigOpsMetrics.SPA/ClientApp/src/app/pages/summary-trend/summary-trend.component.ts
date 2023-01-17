import {
  Component,
  OnInit,
  AfterViewInit,
  OnDestroy,
  TemplateRef,
  ContentChild,
} from "@angular/core";
import { Subscription } from "rxjs";
import { Graph } from "src/app/models/graph";
import { Metrics } from "src/app/models/metrics";
import { FilterService } from "src/app/services/filter.service";
import { MetricsService } from "src/app/services/metrics.service";
import { Colors } from "src/app/models/colors";
import { environment } from "src/environments/environment";

@Component({
  selector: "app-summary-trend",
  templateUrl: "./summary-trend.component.html",
  styleUrls: ["./summary-trend.component.css"],
})
export class SummaryTrendComponent implements OnInit, AfterViewInit, OnDestroy {
  @ContentChild(TemplateRef) template: TemplateRef<any>;
  filterState: any;
  private filterSubscription: Subscription;
  private metricsSubscription: Subscription;
  colors = new Colors();
  data: any;
  tpGraphMetrics = new Metrics({
    measure: "tp",
  });
  tpGraph: Graph = {
    x: "month",
    y: "vph",
    hoverTemplate: "<b>%{x}</b>" + ", <b>%{y:.0f}</b>" + "<extra></extra>",
    lineColor: this.colors.sigOpsGreen,
  };
  tpTitle = "Throughput";

  aogGraphMetrics = new Metrics({
    measure: "aogd",
    formatDecimals: 1,
    formatType: "percent",
  });
  aogGraph: Graph = {
    x: "month",
    y: "aog",
    hoverTemplate: "<b>%{x}</b>" + ", <b>%{y:.1%}</b>" + "<extra></extra>",
    lineColor: this.colors.sigOpsGreen,
  };
  aogTitle = "Arrivals on Green";

  prdGraphMetrics = new Metrics({
    measure: "prd",
    formatDecimals: 2,
  });
  prdGraph: Graph = {
    x: "month",
    y: "pr",
    hoverTemplate: "<b>%{x}</b>" + ", <b>%{y:.2f}</b>" + "<extra></extra>",
    lineColor: this.colors.sigOpsGreen,
  };
  prdTitle = "Progression Ratio";

  qsdGraphMetrics = new Metrics({
    measure: "qsd",
    formatDecimals: 1,
    formatType: "percent",
  });
  qsdGraph: Graph = {
    x: "month",
    y: "qs_freq",
    hoverTemplate: "<b>%{x}</b>" + ", <b>%{y:.1%}</b>" + "<extra></extra>",
    lineColor: this.colors.sigOpsGreen,
  };
  qsdTitle = "Queue Spillback";

  sfdTitle = "Peak Period Split Failure";
  sfdGraphMetrics = new Metrics({
    measure: "sfd",
    formatDecimals: 2,
    formatType: "percent",
  });
  sfGraph: Graph = {
    x: "month",
    y: "sf_freq",
    hoverTemplate: "<b>%{x}</b>" + ", <b>%{y:.2%}</b>" + "<extra></extra>",
    lineColor: this.colors.sigOpsGreen,
  };

  sfoTitle = "Off-Peak Split Failure";
  sfoGraphMetrics = new Metrics({
    measure: "sfo",
    formatDecimals: 2,
    formatType: "percent",
  });

  ttiTitle = "Travel Time Index";
  ttiGraphMetrics = new Metrics({
    measure: "tti",
    formatDecimals: 2,
    goal: environment.ttiGoal,
  });
  ttiGraph: Graph = {
    x: "month",
    y: "tti",
    hoverTemplate: "<b>%{x}</b>" + ", <b>%{y:.2f}</b>" + "<extra></extra>",
    lineColor: this.colors.sigOpsGreen,
  };

  ptiTitle = "Planning Time Index";
  ptiGraphMetrics = new Metrics({
    measure: "pti",
    formatDecimals: 2,
    goal: environment.ptiGoal,
  });
  ptiGraph: Graph = {
    x: "month",
    y: "pti",
    hoverTemplate: "<b>%{x}</b>" + ", <b>%{y:.2f}</b>" + "<extra></extra>",
    lineColor: this.colors.sigOpsGreen,
  };

  dtvTitle = "Daily Volume";
  dtvGraphMetrics = new Metrics({
    measure: "vpd",
  });
  dtvGraph: Graph = {
    x: "month",
    y: "vpd",
    hoverTemplate: "<b>%{x}</b>" + ", <b>%{y:.0f}</b>" + "<extra></extra>",
    lineColor: this.colors.sigOpsBlue,
  };

  amvTitle = "AM Hourly Volume";
  amvGraphMetrics = new Metrics({
    measure: "vphpa",
  });
  amvGraph: Graph = {
    x: "month",
    y: "vph",
    hoverTemplate: "<b>%{x}</b>" + ", <b>%{y:.0f}</b>" + "<extra></extra>",
    lineColor: this.colors.sigOpsBlue,
  };

  pmvTitle = "PM Hourly Volume";
  pmvGraphMetrics = new Metrics({
    measure: "vphpp",
  });
  pmvGraph: Graph = {
    x: "month",
    y: "vph",
    hoverTemplate: "<b>%{x}</b>" + ", <b>%{y:.0f}</b>" + "<extra></extra>",
    lineColor: this.colors.sigOpsBlue,
  };

  paTitle = "Pedestrian Activations";
  paGraphMetrics = new Metrics({
    measure: "papd",
  });
  paGraph: Graph = {
    x: "month",
    y: "uptime",
    hoverTemplate: "<b>%{x}</b>" + ", <b>%{y:.0f}</b>" + "<extra></extra>",
    lineColor: this.colors.sigOpsBlue,
  };

  duTitle = "Detector Uptime";
  duGraphMetrics = new Metrics({
    measure: "du",
    formatDecimals: 1,
    formatType: "percent",
    goal: environment.duGoal,
  });
  duGraph: Graph = {
    x: "month",
    y: "uptime",
    hoverTemplate: "<b>%{x}</b>" + ", <b>%{y:.1%}</b>" + "<extra></extra>",
    lineColor: this.colors.sigOpsRed,
  };

  pauTitle = "Ped Pushbutton Uptime";
  pauGraphMetrics = new Metrics({
    measure: "pau",
    formatDecimals: 1,
    formatType: "percent",
    goal: environment.ppuGoal,
  });
  pauGraph: Graph = {
    x: "month",
    y: "uptime",
    hoverTemplate: "<b>%{x}</b>" + ", <b>%{y:.1%}</b>" + "<extra></extra>",
    lineColor: this.colors.sigOpsRed,
  };

  cctvTitle = "CCTV Uptime";
  cctvGraphMetrics = new Metrics({
    measure: "cctv",
    formatDecimals: 1,
    formatType: "percent",
    goal: environment.cctvGoal,
  });
  cctvGraph: Graph = {
    x: "month",
    y: "uptime",
    hoverTemplate: "<b>%{x}</b>" + ", <b>%{y:.1%}</b>" + "<extra></extra>",
    lineColor: this.colors.sigOpsRed,
  };

  cuTitle = "Detector Uptime";
  cuGraphMetrics = new Metrics({
    measure: "cu",
    formatDecimals: 1,
    formatType: "percent",
    goal: environment.cuGoal,
  });
  cuGraph: Graph = {
    x: "month",
    y: "uptime",
    hoverTemplate: "<b>%{x}</b>" + ", <b>%{y:.1%}</b>" + "<extra></extra>",
    lineColor: this.colors.sigOpsRed,
  };
  constructor(
    private readonly filterService: FilterService,
    private readonly metricsService: MetricsService
  ) {}

  ngOnInit(): void {
    this.filterService.updateFilterErrorState(1);
    this.filterSubscription = this.filterService.filters.subscribe(
      (filter) => {
        this.data = undefined;
        this.filterState = filter;
        this.metricsSubscription = this.metricsService
          .summaryTrend(this.tpGraphMetrics, filter)
          .subscribe((response) => {
            this.data = response;
            this.loadData();
            this.metricsSubscription.unsubscribe();
          });
      }
    );
  }

  private loadData() {}

  ngAfterViewInit() {}

  ngOnDestroy(): void {
    this.filterSubscription.unsubscribe();
    this.metricsSubscription.unsubscribe();
  }
}
