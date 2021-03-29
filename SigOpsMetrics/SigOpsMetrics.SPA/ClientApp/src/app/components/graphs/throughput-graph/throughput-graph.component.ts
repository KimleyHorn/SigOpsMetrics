import { Component, OnInit } from '@angular/core';
import { FilterService } from 'src/app/services/filter.service';
import { MetricsService } from 'src/app/services/metrics.service';
import { SignalsService } from 'src/app/services/signals.service';

@Component({
  selector: 'app-throughput-graph',
  templateUrl: './throughput-graph.component.html',
  styleUrls: ['./throughput-graph.component.css']
})
export class ThroughputGraphComponent implements OnInit {
  source: string = 'main';
  level: string = 'cor';
  interval: string = 'mo';
  measure: string = 'tp';

  dt = new Date();
  start: string = (this.dt.getMonth() + 1) + '/' + (this.dt.getFullYear() - 1); 
  end: string = this.convertDate(this.dt);

  data: any[] = [];
  corridors: any;

  lineGraph: any = {
    data: [],
    layout: { 
      title: 'Throughput (peak veh/hr)',
      showlegend: false,
      xaxis: {
        title: 'Vehicles per Hour Trend'
      }
    }
  };

  barGraph: any = {
    data: [],
    layout: { 
      showlegend: false,
      xaxis: {
        title: 'Vehicles per Hour Trend'
      }
    }
  };

  constructor(private metricsService: MetricsService, private signalsService: SignalsService, private filtersService: FilterService) { }

  ngOnInit(): void {
    this.metricsService.getMetrics(this.source, this.level, this.interval, this.measure, this.start, this.end).subscribe(response => {
      this.data = response;
      this.corridors = new Set(this.data.filter(value => value['corridor'] !== null && value['zone_Group'] === 'All RTOP').map(data => data['corridor']));
      console.log(response);

      this.loadBarGraph();
      this.loadLineGraph();
    });
  }

  loadLineGraph(){
    let graphData: any[] = [];
    this.corridors.forEach(corridor => {
      let filterData = this.data.filter(data => data['corridor'] === corridor);
      let trace = {
        name: corridor,
        x: filterData.map(value => new Date(value['month'])),
        y: filterData.map(value => value['vph']),
        text: filterData.map(value => value['corridor']),
        hovertemplate:
          '<b>%{text}</b>' +
          '<br>Week of: <b>%{x}</b>' +
          '<br>Throughput (peak veh/hr): <b>%{y}</b>' +
          '<extra></extra>',
        mode: 'lines'
      };

      graphData.push(trace);
    });

    console.log(graphData);

    this.lineGraph.data = graphData;
  }

  loadBarGraph(){
    let graphData: any[] = [];
    let currentMonth = new Date().getMonth();

    this.corridors.forEach(corridor => {
      let monthData = this.data.filter(data => data['corridor'] === corridor && new Date(data['month']).getMonth() === currentMonth);
      let trace = {
        name: corridor,
        x: monthData.map(value => value['vph']),
        y: monthData.map(value => value['corridor']),
        orientation: 'h',
        type: 'bar',
        hovertemplate:
          '<b>%{y}</b>' +
          '<br>Throughput (peak veh/hr): <b>%{x}</b>' +
          '<extra></extra>',
      };

      graphData.push(trace);
    });

    this.barGraph.data = graphData;
  }

  convertDate(date: Date): string{
    return (date.getMonth() + 1) + "/" + date.getFullYear();
  }
} 
