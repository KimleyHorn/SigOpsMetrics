//Angular components
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
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
import { RampMetersComponent } from './pages/ramp-meters/ramp-meters.component';

import { HeaderComponent } from './core/header/header.component';
import { MapComponent } from './components/map/map.component';
import { ChartToggleComponent } from './components/toggles/chart-toggle/chart-toggle.component';
import { FilterSidenavComponent } from './core/filter-sidenav/filter-sidenav.component';
import { MetricCardComponent } from './components/cards/metric-card/metric-card.component';
import { BarLineGraphComponent } from './components/graphs/bar-line-graph/bar-line-graph.component';
import { GraphDashboardComponent} from './components/dashboards/graph-dashboard/graph-dashboard.component';

const routes = [
  { text: 'Dashboard', icon: 'insert_chart', path: '', component: DashboardComponent, pathMatch: 'full' },
  { text: 'Operations', icon: 'toys', path: 'operations', component: OperationsComponent},
  { text: 'Maintenance', icon: 'bar_chart', path: 'maintenance', component: MaintenanceComponent},
  { text: 'Watchdog', icon: 'alarm', path: 'watchdog', component: WatchdogComponent},
  { text: 'TEAMS Tasks', icon: 'build', path: 'teams-tasks', component: TeamsTasksComponent},
  { text: 'Reports', icon: 'receipt', path: 'reports', component: ReportsComponent},
  { text: 'Health Metrics', icon: 'healing', path: 'health-metrics', component: HealthMetricsComponent},
  { text: 'Ramp Meters', icon: 'traffic', path: 'ramp-meters', component: RampMetersComponent},
  { text: 'Signal Info', icon: 'help', path: 'signal-info', component: SignalInfoComponent}
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
    RampMetersComponent,
    ChartToggleComponent,
    FilterSidenavComponent,
    MetricCardComponent,
    BarLineGraphComponent,
    GraphDashboardComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot(routes,
    { relativeLinkResolution: 'legacy' }),
    BrowserAnimationsModule,
    MaterialModule,
    GoogleMapsModule,
    PlotlyViaCDNModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
//export class MaterialModule {}
export class AppModule { }
