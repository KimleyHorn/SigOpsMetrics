import { Component, OnInit } from '@angular/core';
import { BaseDashboardComponent } from '../base-dashboard/base-dashboard.component';

@Component({
  selector: 'app-metrics-dashboard',
  templateUrl: './metrics-dashboard.component.html',
  styleUrls: ['./metrics-dashboard.component.css']
})
export class MetricsDashboardComponent extends BaseDashboardComponent implements OnInit {
  ngOnInit(): void {
    super.ngOnInit();
  }
}
