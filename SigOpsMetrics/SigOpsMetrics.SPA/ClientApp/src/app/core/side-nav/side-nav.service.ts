import { Injectable } from '@angular/core';
import { MatGridTileHeaderCssMatStyler } from '@angular/material/grid-list';
import { MatSidenav } from '@angular/material/sidenav';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SideNavService {
  private sideNav: MatSidenav;
  private _isExpanded: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(true);
  public isExpanded = this._isExpanded.asObservable();

  constructor() {}

  public setSideNav(sideNav: MatSidenav){
    this.sideNav = sideNav;
  }

  public toggle(): void{
    this._isExpanded.next(!this._isExpanded.value);
  }

}
