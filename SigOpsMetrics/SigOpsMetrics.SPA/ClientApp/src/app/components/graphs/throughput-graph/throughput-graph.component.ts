import { Component, OnInit } from '@angular/core';
import { MetricsService } from 'src/app/services/metrics.service';
import { SignalsService } from 'src/app/services/signals.service';

@Component({
  selector: 'app-throughput-graph',
  templateUrl: './throughput-graph.component.html',
  styleUrls: ['./throughput-graph.component.css']
})
export class ThroughputGraphComponent implements OnInit {
  data:any[];

  source: string = "main";
  level: string = "cor";
  interval: string = "mo";
  measure: string = "tp";

  dt = new Date();
  start: string = (this.dt.getMonth() + 1) + "/" + (this.dt.getFullYear() - 1); 
  end: string = this.convertDate(this.dt);

  public graph = {
    data: [
        { x: [1, 2, 3], y: [2, 6, 3], type: 'scatter', mode: 'lines+points', marker: {color: 'red'} },
        { x: [1, 2, 3], y: [2, 5, 3], type: 'bar' },
    ],
    layout: {width: 320, height: 240, title: 'A Fancy Plot'}
  };

  constructor(private metricsService: MetricsService, private signalsService: SignalsService) { }

  ngOnInit(): void {
    // this.signalsService.getData("corridors").subscribe(response => {

    // });
    
    this.getData();
    
  }

  getData(){
    this.metricsService.getMetrics(this.source, this.level, this.interval, this.measure, this.start, this.end).subscribe(response => {
      this.data = response;
      console.log(response);
    });
  }

  unpack(rows, key){
    return rows.map(function(row) { return row[key]; });
  }

  convertDate(date: Date): string{
    return (date.getMonth() + 1) + "/" + date.getFullYear();
  }
} 
