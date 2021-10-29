import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { FilterService } from 'src/app/services/filter.service';
import { HelpService } from 'src/app/services/help.service';

@Component({
  selector: 'help',
  templateUrl: './help.component.html',
  styleUrls: ['./help.component.css'],
  encapsulation: ViewEncapsulation.None
})


export class HelpComponent implements OnInit {

  data = [];
  constructor(private titleService:Title, private helpService: HelpService, private filterService:FilterService) { }

  ngOnInit() {
    this.titleService.setTitle("SigOpsMetrics - About");
    this.data = this.helpService.getAllHelpData().filter(d => d.panel === true);
    this.filterService.updateFilterErrorState(3);
  }
}
