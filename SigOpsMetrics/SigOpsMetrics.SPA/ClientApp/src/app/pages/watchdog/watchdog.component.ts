import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-watchdog',
  templateUrl: './watchdog.component.html',
  styleUrls: ['./watchdog.component.css']
})
export class WatchdogComponent implements OnInit {

  constructor(private titleService:Title) { }

  ngOnInit(): void {
    this.titleService.setTitle("SigOpsMetrics - Watchdog")

  }

}
