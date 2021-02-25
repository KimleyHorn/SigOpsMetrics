import { Component, Input, OnInit } from '@angular/core';
import { SideNavService } from '../side-nav/side-nav.service';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit {
  
  constructor(private sideNav: SideNavService) { }

  ngOnInit(): void {
  }

  toggleSideNav(){
    this.sideNav.toggle();
  }
}
