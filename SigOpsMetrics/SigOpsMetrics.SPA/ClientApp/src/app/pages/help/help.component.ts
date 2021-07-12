import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { HelpService } from 'src/app/services/help.service';

@Component({
  selector: 'help',
  templateUrl: './help.component.html',
  styleUrls: ['./help.component.css'],
  encapsulation: ViewEncapsulation.None
})


export class HelpComponent implements OnInit {

  data = [];
  constructor(private titleService:Title, private helpService: HelpService) { }

  ngOnInit() {
    this.titleService.setTitle("SigOpsMetrics - Help");
    this.data = this.helpService.getAllHelpData().filter(d => d.panel === true);
  }
}
