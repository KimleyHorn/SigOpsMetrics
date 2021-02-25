import { Injectable } from '@angular/core';
import { MatSidenav } from '@angular/material/sidenav';

@Injectable({
  providedIn: 'root'
})
export class SideNavService {
  private sideNav: MatSidenav;

  constructor() { }

  public setSideNav(sideNav: MatSidenav){
    this.sideNav = sideNav;
  }

  public open(){
    return this.sideNav.open();
  }

  public close(){
    return this.sideNav.close();
  }

  public toggle(): void{
    this.sideNav.toggle();
  }
}
