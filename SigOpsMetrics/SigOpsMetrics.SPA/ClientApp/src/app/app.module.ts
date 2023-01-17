//Angular components
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { GoogleMapsModule } from '@angular/google-maps';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MaterialModule } from './modules/material-module';

import { PlotlyViaCDNModule } from 'angular-plotly.js';
PlotlyViaCDNModule.setPlotlyVersion('latest');

import { AppComponent } from './app.component';
import { SideNavComponent } from './core/side-nav/side-nav.component';

//Pages
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { SignalInfoComponent } from './pages/signal-info/signal-info.component';
import { OperationsComponent } from './pages/operations/operations.component';
import { MaintenanceComponent } from './pages/maintenance/maintenance.component';
import { WatchdogComponent } from './pages/watchdog/watchdog.component';
import { TeamsTasksComponent } from './pages/teams-tasks/teams-tasks.component';
import { ReportsComponent } from './pages/reports/reports.component';
import { HealthMetricsComponent } from './pages/health-metrics/health-metrics.component';

import { HeaderComponent } from './core/header/header.component';
import { MapComponent } from './components/map/map.component';
import { ChartToggleComponent } from './components/toggles/chart-toggle/chart-toggle.component';
import { FilterSidenavComponent } from './core/filter-sidenav/filter-sidenav.component';
import { MetricCardComponent } from './components/cards/metric-card/metric-card.component';
import { CircleProgressComponent } from './components/graphs/circle-progress/circle-progress.component';
import { BarLineGraphComponent } from './components/graphs/bar-line-graph/bar-line-graph.component';
import { LineGraphComponent } from "./components/graphs/line-graph/line-graph.component";
import { BarLineLineGraphComponent } from "./components/graphs/bar-line-line-graph/bar-line-line-graph.component";
import { GraphDashboardComponent } from "./components/dashboards/graph-dashboard/graph-dashboard.component";
import { GraphNoCardDashboardComponent } from "./components/dashboards/graph-no-card-dashboard/graph-no-card-dashboard.component";
import { DatePipe } from "@angular/common";
import { ScatterMapComponent } from "./components/maps/scatter-map/scatter-map.component";
import { BaseDashboardComponent } from "./components/dashboards/base-dashboard/base-dashboard.component";
import { MapDashboardComponent } from "./components/dashboards/map-dashboard/map-dashboard.component";
import { MetricSelectComponent } from "./components/selects/metric-select/metric-select.component";
import { TicketsGraphComponent } from "./components/graphs/tickets-graph/tickets-graph.component";
import { DashboardTableComponent } from "./components/tables/dashboard-table/dashboard-table.component";
import { TicketsTableComponent } from "./components/tables/tickets-table/tickets-table.component";
import { ContactComponent } from "./core/contact-form/contact-form";
import { NgxMaskModule } from "ngx-mask";
import { FilterChipListComponent } from "./components/chip-lists/filter-chip-list/filter-chip-list.component";
import { HelpComponent } from "./pages/help/help.component";
import { HelpPanelComponent } from "./components/panels/help-panel/help-panel.component";
import { ExcelExportComponent } from "./components/excel-export/excel-export.component";
import { GlobalHttpInterceptorService } from "./services/global-http-interceptor.service";
import { NgCircleProgressModule } from "ng-circle-progress";
import { SummaryTrendComponent } from "./pages/summary-trend/summary-trend.component";

const routes = [
  {
    text: "Dashboard",
    icon: "insert_chart",
    path: "",
    component: DashboardComponent,
    pathMatch: "full",
  },
  {
    text: "Operations",
    icon: "toys",
    path: "operations",
    component: OperationsComponent,
  },
  {
    text: "Maintenance",
    icon: "bar_chart",
    path: "maintenance",
    component: MaintenanceComponent,
  },
  {
    text: "Watchdog",
    icon: "alarm",
    path: "watchdog",
    component: WatchdogComponent,
  },
  {
    text: "TEAMS Tasks",
    icon: "build",
    path: "teams-tasks",
    component: TeamsTasksComponent,
  },
  {
    text: "Reports",
    icon: "receipt",
    path: "reports",
    component: ReportsComponent,
  },
  {
    text: "Health Metrics",
    icon: "healing",
    path: "health-metrics",
    component: HealthMetricsComponent,
  },
  {
    text: "Summary Trend",
    icon: "show_chart",
    path: "summary-trend",
    component: SummaryTrendComponent,
  },
  {
    text: "Signal Info",
    icon: "info",
    path: "signal-info",
    component: SignalInfoComponent,
  },
  {
    text: "About",
    icon: "help",
    path: "about",
    component: HelpComponent,
  },
];

@NgModule({
  declarations: [
    AppComponent,
    SideNavComponent,
    DashboardComponent,
    HeaderComponent,
    MapComponent,
    SignalInfoComponent,
    OperationsComponent,
    MaintenanceComponent,
    WatchdogComponent,
    TeamsTasksComponent,
    ReportsComponent,
    HealthMetricsComponent,
    ChartToggleComponent,
    FilterSidenavComponent,
    MetricCardComponent,
    CircleProgressComponent,
    BarLineGraphComponent,
    LineGraphComponent,
    BarLineLineGraphComponent,
    GraphDashboardComponent,
    // GraphsDashboardComponent,
    GraphNoCardDashboardComponent,
    ScatterMapComponent,
    BaseDashboardComponent,
    MapDashboardComponent,
    MetricSelectComponent,
    TicketsGraphComponent,
    DashboardTableComponent,
    TicketsTableComponent,
    ContactComponent,
    FilterChipListComponent,
    HelpComponent,
    HelpPanelComponent,
    ExcelExportComponent,
    SummaryTrendComponent,
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: "ng-cli-universal" }),
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forRoot(routes, { relativeLinkResolution: "legacy" }),
    BrowserAnimationsModule,
    MaterialModule,
    GoogleMapsModule,
    PlotlyViaCDNModule,
    NgxMaskModule.forRoot(),
    NgCircleProgressModule.forRoot({
      radius: 100,
      outerStrokeWidth: 16,
      innerStrokeWidth: 8,
      outerStrokeColor: "#78C000",
      innerStrokeColor: "#C7E596",
      animationDuration: 300,
    }),
  ],
  providers: [
    DatePipe,
    ContactComponent,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: GlobalHttpInterceptorService,
      multi: true,
    },
  ],
  bootstrap: [AppComponent],
})
//export class MaterialModule {}
export class AppModule {}
