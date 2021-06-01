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
    {url:"https://traffic.dot.ga.gov/atspm", name:"ATSPM", icon: "..\\assets\\images\\icon_atspm.png"},
    {url:"http://www.511ga.org/", name:"Georgia 511", icon: "..\\assets\\images\\icon_georgia511.png"},
    {url:"http://gdot-tmc-maxv/maxview/", name:"MaxView", icon: "..\\assets\\images\\icon_maxview.png"},
    {url:"https://navigator-atms.dot.ga.gov/", name:"Navigator", icon: "..\\assets\\images\\icon_navigator.jpg"},
    {url:"https://designitapps.com/GDOT/", name:"TEAMS", icon: "..\\assets\\images\\icon_teams.png"},
    {url:"https://ritis.org/", name:"RITIS", icon: "..\\assets\\images\\icon_ritis.jpg"},
    {url:"https://gdotcitrix.dot.ga.gov/vpn/index.html", name:"GDOT Citrix", icon: "..\\assets\\images\\icon_citrix.png"},
    {url:"http://mygdot.dot.ga.gov/", name:"MyGDOT", icon: "..\\assets\\images\\icon_gdot.png"},
]

  ngOnInit(): void {
  }

  toggleSideNav(){
    this.sideNav.toggle();
  }
}
