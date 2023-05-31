import { Component, OnInit, ViewChild } from "@angular/core";
import { MatSidenav } from "@angular/material/sidenav";
import { Router } from "@angular/router";
import { FilterService } from "src/app/services/filter.service";
import { SideNavService } from "../side-nav/side-nav.service";
import { AppConfig } from "src/app/app.config";

@Component({
  selector: 'app-side-nav',
  templateUrl: './side-nav.component.html',
  styleUrls: ['./side-nav.component.css']
})

export class SideNavComponent implements OnInit {
  @ViewChild('sidenav', {static: true}) public sideNav: MatSidenav;

  public menuItems: Array<any> = [];
  public isExpanded: boolean = true;
  public filterIsExpanded: boolean = false;
  public buttonColor = 'primary';
  public isDisabled = false;
  constructor(private router: Router, private sideNavService: SideNavService, public filterService: FilterService){}

  ngOnInit(): void {
    this.sideNavService.setSideNav(this.sideNav);
    var routes = this.router.config.filter(item => item.data["text"] !== undefined && item.data["text"] !== "" && this.filterRoute(item.data));

    this.menuItems = this.mapItems(routes);

    this.sideNavService.isExpanded.subscribe((value) => {
      this.isExpanded = value;
    })

    this.filterService.errorState.subscribe((value) => {
      if (value == 1) {
        this.buttonColor ='primary';
        this.isDisabled = false;
      } else if (value == 2) {
        this.buttonColor ='warn';
        this.isDisabled = false;
      } else {
        this.buttonColor = 'disabled';
        this.filterIsExpanded = false;
        this.isDisabled = true;
      }
    })

  }
  public mapItems(routes: any[]){
    var items: any[] = [];
    routes.forEach(route => {
      var item = {
        text: route.data.text,
        icon: route.data.icon,
        path: route.path ? route.path : ''
      };

      items.push(item);
    });

    return items;
  }

  toggleFilter() {
    this.filterIsExpanded = !this.filterIsExpanded;
  }

  filterRoute(r){
    switch (r["text"]) {
      case "Operations":
        return AppConfig.settings.hasPageOperations;
      case "Maintenance":
        return AppConfig.settings.hasPageMaintenance;
      case "Watchdog":
        return AppConfig.settings.hasPageWatchdog;
      case "TEAMS Tasks":
        return AppConfig.settings.hasPageTeamsTasks;
      case "Reports":
        return AppConfig.settings.hasPageReports;
      case "Health Metrics":
        return AppConfig.settings.hasPageHealthMetrics;
      case "Summary Trend":
        return AppConfig.settings.hasPageSummaryTrend;
    }
    return true;
  }
}
