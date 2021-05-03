import { Component, ContentChild, Input, OnInit, TemplateRef } from '@angular/core';
import { Metrics } from 'src/app/models/metrics';
import { SignalInfo } from 'src/app/models/signal-info';
import { FilterService } from 'src/app/services/filter.service';
import { FormatService } from 'src/app/services/format.service';
import { MetricsService } from 'src/app/services/metrics.service';
import { BaseDashboardComponent } from '../base-dashboard/base-dashboard.component';

@Component({
  selector: 'app-graph-dashboard',
  templateUrl: './graph-dashboard.component.html',
  styleUrls: ['./graph-dashboard.component.css']
})
export class GraphDashboardComponent extends BaseDashboardComponent implements OnInit {

    ngOnInit(): void {
      super.ngOnInit();
    }

}
