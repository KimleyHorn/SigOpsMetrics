import { Component, Input, OnInit } from '@angular/core';
import { Graph } from 'src/app/models/graph';
import { Metrics } from 'src/app/models/metrics';
import { FilterService } from 'src/app/services/filter.service';
import { MetricsService } from 'src/app/services/metrics.service';

@Component({
  selector: 'app-line-bar-graph',
  templateUrl: './line-bar-graph.component.html',
  styleUrls: ['./line-bar-graph.component.css']
})
export class LineBarGraphComponent implements OnInit {
  private _currentMonth = new Date().getMonth();

  @Input() title: string = "";
  @Input() metrics: Metrics;
  corridors: any;
  data: any;
  filteredData: any;

  @Input() line: Graph;
  lineGraph: any;
  lineData: any;

  //bar graph input values
  @Input() bar: Graph;
  barGraph: any;
  barData: any;

  defaultColor: string = '#A9A9A9';
  selectColor: string = 'red';

  constructor(private _filterService: FilterService, private _metricsService: MetricsService) {}

  ngOnInit(): void {
    this.barGraph = {
      data: [],
      layout: { 
        showlegend: false,
        xaxis: {
          title: this.bar.title,
          tickangle: 90
        },
        yaxis:{
          automargin: true,
        },
        hovermode: 'closest'
      }
    };

    this.lineGraph = {
      data: [],
      layout: { 
        showlegend: false,
        xaxis: {
          title: this.line.title
        },
        yaxis:{
          automargin: true,
        },
        hovermode: 'closest'
      }
    };

    this._metricsService.getMetrics(this.metrics).subscribe(response => {
      //this.filteredData = response;
      this.data = response;
      this.filteredData = response;
      this.corridors = new Set(this.data.filter(value => value['corridor'] !== null).map(data => data['corridor']).slice(0, 20));
      this._loadGraphs();
    });

    this._filterService.filters.subscribe(filter => {
      if(this.data !== undefined){
        this.filteredData = this.data.filter(value => value['zone_Group'] === filter.zoneGroup);

        this._loadGraphs();
      }
    });
  }

  private _loadGraphs(){
    this.lineData = this.filteredData;
    //TODO: adjusted this filter to be based on the selected month
    this.barData = this.filteredData.filter(dataItem => new Date(dataItem['month']).getMonth() === this._currentMonth);
    
    this._loadBarGraph();
    this._loadLineGraph();
  }

  private _loadBarGraph(){
    let graphData: any[] = [];

    if(this.corridors !== undefined){
      this.corridors.forEach(corridor => {
        //TODO: need to determine if this data should be handled separately
        let monthData = this.barData.filter(data => data['corridor'] === corridor);
        let trace = {
          name: corridor,
          x: monthData.map(value => value[this.bar.x]),
          y: monthData.map(value => value[this.bar.y]),
          orientation: 'h',
          type: 'bar',
          hovertemplate: this.bar.hoverTemplate,
          marker: {
            color: this.defaultColor
          }
        };

        graphData.push(trace);
      });
    }

    this.barGraph.data = graphData;
  }

  private _loadLineGraph(){
    let graphData: any[] = [];
    if(this.corridors !== undefined){
      this.corridors.forEach(corridor => {
        let lineData = this.lineData.filter(data => data['corridor'] === corridor);
        let trace = {
          name: corridor,
          x: lineData.map(value => new Date(value[this.line.x])),
          y: lineData.map(value => value[this.line.y]),
          text: lineData.map(value => value[this.line.text]),
          hovertemplate: this.line.hoverTemplate,
          mode: 'lines',
          line: {
            color: this.defaultColor
          }
        };
  
        graphData.push(trace);
      });
    }

    this.lineGraph.data = graphData;
  }

  graphClicked(e){
    this._resetColor();
    
    var name = e.points[0].data.name;
    this._selectColor(name);
  }

  private _selectColor(name: string){
    this.barGraph.data
    .filter(item => item.name === name)
    .map(dataItem => {
      dataItem.marker.color = this.selectColor;
      return dataItem;
    });

    this.lineGraph.data
    .filter(item => item.name === name)
    .map(dataItem => {
      dataItem.line.color = this.selectColor;
      return dataItem;
    });
  }

  private _resetColor(){
    this.barGraph.data.map(dataItem => {
      dataItem.marker.color = this.defaultColor;
      return dataItem;
    });

    this.lineGraph.data.map(dataItem => {
      dataItem.line.color = this.defaultColor;
      return dataItem;
    });
  }
}
