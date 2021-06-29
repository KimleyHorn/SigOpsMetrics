import { Component } from '@angular/core';
import { Title } from '@angular/platform-browser';
import{Router, NavigationEnd} from '@angular/router';

declare let gtag: Function;

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})

export class AppComponent  {
  title = 'app';
  constructor(public router: Router, private titleService: Title){
    this.router.events.subscribe(event => {
      if(event instanceof NavigationEnd){
        gtag('config', 'G-8WB20C5SBR',
          {
            'page_path': event.urlAfterRedirects
          }
        );
      }
    }
  )}
  public setTitle(newTitle: string) {
    this.titleService.setTitle(newTitle);
  }
  public getTitle(){
    this.titleService.getTitle();
  }
}
