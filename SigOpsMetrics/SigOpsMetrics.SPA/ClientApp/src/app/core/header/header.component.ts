import { Component, Input, OnInit, ViewEncapsulation } from '@angular/core';
import { SideNavService } from '../side-nav/side-nav.service';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class HeaderComponent implements OnInit {
  
  constructor(private sideNav: SideNavService) { }
  links: any[] = [
    {url:"https://traffic.dot.ga.gov/atspm", name:"ATSPM"},
    {url:"http://www.511ga.org/", name:"Georgia 511"},
    {url:"http://gdot-tmc-maxv/maxview/", name:"MaxView"},
    {url:"https://navigator-atms.dot.ga.gov/", name:"Navigator"},
    {url:"https://designitapps.com/GDOT/", name:"TEAMS"},
    {url:"https://ritis.org/", name:"RITIS"},
    {url:"https://gdotcitrix.dot.ga.gov/vpn/index.html", name:"GDOT Citrix"},
    {url:"http://mygdot.dot.ga.gov/", name:"MyGDOT"},
]

  ngOnInit(): void {
  }

  toggleSideNav(){
    this.sideNav.toggle();
  }
}
