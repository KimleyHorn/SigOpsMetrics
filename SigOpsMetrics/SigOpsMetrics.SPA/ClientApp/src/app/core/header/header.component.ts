import { Component, Input, OnInit, ViewEncapsulation } from '@angular/core';
import { HelpService } from 'src/app/services/help.service';
import { ContactComponent } from '../contact-form/contact-form';
import { SideNavService } from '../side-nav/side-nav.service';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css'],
  encapsulation: ViewEncapsulation.None,
  providers: [ContactComponent]
})
export class HeaderComponent implements OnInit {
  helpData: any = "";
  constructor(private sideNav: SideNavService, private contact: ContactComponent, private helpService: HelpService) { }
  links: any[] = [
    {url:"https://traffic.dot.ga.gov/atspm", name:"ATSPM", icon: "..\\assets\\images\\icon_atspm.png"},
    {url:"https://gdotcitrix.dot.ga.gov/vpn/index.html", name:"GDOT Citrix", icon: "..\\assets\\images\\icon_citrix.png"},
    {url:"http://www.511ga.org/", name:"Georgia 511", icon: "..\\assets\\images\\icon_gdot511.png"},
    {url:"http://gdot-tmc-maxv/maxview/", name:"MaxView", icon: "..\\assets\\images\\icon_maxview.png"},
    {url:"https://navigator-atms.dot.ga.gov/", name:"Navigator", icon: "..\\assets\\images\\icon_navigator.jpg"},
    {url:"https://ritis.org/", name:"RITIS", icon: "..\\assets\\images\\icon_ritis.jpg"},
    {url:"https://designitapps.com/GDOT/", name:"TEAMS", icon: "..\\assets\\images\\icon_teams.png"}
  ]

  ngOnInit(): void {
  }

  toggleSideNav(){
    this.sideNav.toggle();
  }

  toggleContact(){
    this.contact.toggle();
  }

  getHelpContent(){
    this.helpData = this.helpService.getHelpData();
  }

}
