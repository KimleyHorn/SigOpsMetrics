import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-circle-progress',
  templateUrl: './circle-progress.component.html',
  styleUrls: ['./circle-progress.component.css']
})

export class CircleProgressComponent implements OnInit{
  @Input() title: string = "";
  @Input() chartData: any;
  @Input() headerColor: string;

  ngOnInit(){
    this.getHeaderColor();
  }

  getHeaderColor(){
    switch (this.title){
      case 'NORTH':
        this.headerColor = '#66cc66';
        break;
      case 'SOUTHEAST':
        this.headerColor = '#fbd349';
        break;
      case 'SOUTHWEST':
        this.headerColor = '#1e7ce6';
        break;
      case 'WESTERN METRO':
        this.headerColor = '#ed3833';
        break;
      case 'CENTRAL METRO':
        this.headerColor = '#071f3e';
        break;
      case 'EASTERN METRO':
        this.headerColor = '#45729e';
        break;
      default:
        this.headerColor = 'gray';
        break;
    }
  }

  getOuterStrokeColor(index: number){
    if (this.chartData[index] > 80)
      return '#78C000';
    else if (this.chartData[index] > 30)
      return '#FFC300';
    else
      return '#D61616';
  }

  getInnerStrokeColor(index: number){
    if (this.chartData[index] > 80)
      return '#C7E596';
    else if (this.chartData[index] > 30)
      return '#F8F49F';
    else
      return '#F79F9F';
  }
}
