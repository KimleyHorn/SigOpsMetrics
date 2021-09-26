import { Component, OnInit, ViewChild } from "@angular/core";
import { MatSidenav } from "@angular/material/sidenav";
import { Router } from "@angular/router";
import { FilterService } from "src/app/services/filter.service";
import { SideNavService } from "../side-nav/side-nav.service";

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
  constructor(private router: Router, private sideNavService: SideNavService, public filterService: FilterService){}
  
  ngOnInit(): void {
    this.sideNavService.setSideNav(this.sideNav);

    var routes = this.router.config.filter(item => item["text"] !== undefined && item["text"] !== "");
    this.menuItems = this.mapItems(routes);

  
    this.sideNavService.isExpanded.subscribe((value) => {
      this.isExpanded = value;
    })

    this.filterService.errorState.subscribe((value) => {
      if (value) {
        this.buttonColor ='warn';
      } else {
        this.buttonColor ='primary';
      }
    })

  }
  public mapItems(routes: any[]){
    var items: any[] = [];
    routes.forEach(route => {
      var item = {
        text: route.text,
        icon: route.icon,
        path: route.path ? route.path : ''
      };

      items.push(item);
    });

    return items;
  }

  toggleFilter() {
    this.filterIsExpanded = !this.filterIsExpanded;
  }
}
