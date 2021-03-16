import { Component, OnInit } from '@angular/core';
import { ChartType } from 'src/app/models/chart-type';
import { ChartToggleService } from './chart-toggle.service';

@Component({
  selector: 'app-chart-toggle',
  templateUrl: './chart-toggle.component.html',
  styleUrls: ['./chart-toggle.component.css']
})
export class ChartToggleComponent implements OnInit {
  selectedValue: string = "1";

  chartTypes: ChartType[] = [
    { chartTypeValue: "1", chartTypeName: "Throughput" },
    { chartTypeValue: "2", chartTypeName: "Arrivals on Green" },
    { chartTypeValue: "3", chartTypeName: "Progression Ratio" },
    { chartTypeValue: "4", chartTypeName: "Spillback Rate" },
    { chartTypeValue: "5", chartTypeName: "Split Failures" },
    { chartTypeValue: "6", chartTypeName: "Travel Time Index" },
    { chartTypeValue: "7", chartTypeName: "Planning Time Index" },
  ];

  constructor(private toggleService: ChartToggleService) { }

  ngOnInit(): void {
    this.toggleService.setValue(this.selectedValue);
  }

  toggleChange(value: string){
    this.toggleService.setValue(value);
  }
}
