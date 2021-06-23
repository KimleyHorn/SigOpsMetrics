import { Component, ContentChild, Input, OnInit, TemplateRef } from '@angular/core';
import { Metrics } from 'src/app/models/metrics';
import { SignalInfo } from 'src/app/models/signal-info';
import { FilterService } from 'src/app/services/filter.service';
import { FormatService } from 'src/app/services/format.service';
import { MetricsService } from 'src/app/services/metrics.service';
import { BaseDashboardComponent } from '../base-dashboard/base-dashboard.component';

@Component({
  selector: 'app-graph-no-card-dashboard',
  templateUrl: './graph-no-card-dashboard.component.html',
  styleUrls: ['./graph-no-card-dashboard.component.css']
})
export class GraphNoCardDashboardComponent extends BaseDashboardComponent implements OnInit {

    ngOnInit(): void {
      super.ngOnInit();
    }

}
