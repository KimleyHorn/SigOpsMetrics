import { Component, OnInit } from '@angular/core';
import { BaseDashboardComponent } from '../base-dashboard/base-dashboard.component';

@Component({
  selector: 'app-map-dashboard',
  templateUrl: './map-dashboard.component.html',
  styleUrls: ['./map-dashboard.component.css']
})
export class MapDashboardComponent extends BaseDashboardComponent implements OnInit {

  ngOnInit(): void {
    super.ngOnInit();
  }

}
