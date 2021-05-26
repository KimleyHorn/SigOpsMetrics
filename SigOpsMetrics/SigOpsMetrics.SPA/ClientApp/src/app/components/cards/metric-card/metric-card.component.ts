import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-metric-card',
  templateUrl: './metric-card.component.html',
  styleUrls: ['./metric-card.component.css']
})
export class MetricCardComponent implements OnInit {
  @Input() metricValue: string;
  @Input() metricChange: string;
  @Input() metricLabel: string;

  constructor() { }

  ngOnInit(): void {
  }

}
