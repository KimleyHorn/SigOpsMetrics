import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { Graph } from 'src/app/models/graph';
import { Metrics } from 'src/app/models/metrics';
import { MetricsService } from 'src/app/services/metrics.service';

@Component({
  selector: 'app-bar-line-line-graph',
  templateUrl: './bar-line-line-graph.component.html',
  styleUrls: ['./bar-line-line-graph.component.css']
})
export class BarLineLineGraphComponent implements OnInit, OnChanges {
  private _currentMonth = new Date().getMonth();

  @Input() additionalMetrics: Metrics;

  @Input() title: string = "";
  corridors: any;
  @Input() data1: any;

  @Input() line1: Graph;
  lineGraph1: any;
  lineData1: any;

  @Input() line2: Graph;
  lineGraph2: any;
  lineData2: any;

  //bar graph input values
  @Input() bar: Graph;
  barGraph: any;
  barData: any;

  defaultColor: string = '#A9A9A9';
  selectColor: string = 'red';

  constructor(private _metricsService: MetricsService) {}

  ngOnInit(): void {
    this._metricsService.getMetrics(this.additionalMetrics).subscribe(response => {
      this.lineData2 = response;

      this._loadGraphs();
    });
  }

  ngOnChanges(changes: SimpleChanges){
    this.barGraph = {
      data: [],
      layout: {
        showlegend: false,
        autosize: true,
        xaxis: {
          title: this.bar.title,
          tickangle: 90
        },
        yaxis:{
          automargin: true,
        },
        margin:{
          t:25
        },
        hovermode: 'closest'
      }
    };

    this.lineGraph1 = {
      data: [],
      layout: {
        showlegend: false,
        xaxis: {
          title: this.line1.title
        },
        yaxis:{
          automargin: true,
        },
        margin:{
          t:25
        },
        hovermode: 'closest'
      }
    };

    this.lineGraph2 = {
      data: [],
      layout: {
        showlegend: false,
        xaxis: {
          title: this.line2.title
        },
        yaxis:{
          automargin: true,
        },
        margin:{
          t:25
        },
        hovermode: 'closest'
      }
    };

      this._loadGraphs();
  }

  private _loadGraphs(){
    if(this.data1 !== undefined && this.lineData2 !== undefined){
      this.lineData1 = this.data1;
      //TODO: adjusted this filter to be based on the selected month
      this.barData = this.data1.filter(dataItem => new Date(dataItem['month']).getMonth() === this._currentMonth);
      this.corridors = new Set(this.data1.filter(value => value['corridor'] !== null).map(data => data['corridor']));

      this._loadBarGraph();
      this._loadLineGraph1();
      this._loadLineGraph2();
    }
  }

  private _loadBarGraph(){
    let graphData: any[] = [];

    if(this.corridors !== undefined){
      let sortedData = this.barData.sort((n1, n2) => n1[this.bar.x] - n2[this.bar.x]);

      sortedData.forEach(sortItem => {
        let trace = {
          name: sortItem['corridor'],
          x: [sortItem[this.bar.x]],
          y: [sortItem[this.bar.y]],
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

  private _loadLineGraph1(){
    let graphData1: any[] = [];
    if(this.corridors !== undefined){
      this.corridors.forEach(corridor => {
        let lineData1 = this.lineData1.filter(data => data['corridor'] === corridor);
        let trace = {
          name: corridor,
          x: lineData1.map(value => new Date(value[this.line1.x])),
          y: lineData1.map(value => value[this.line1.y]),
          text: lineData1.map(value => value[this.line1.text]),
          hovertemplate: this.line1.hoverTemplate,
          mode: 'lines',
          line: {
            color: this.defaultColor
          }
        };

        graphData1.push(trace);
      });
    }

    this.lineGraph1.data = graphData1;
  }

  private _loadLineGraph2(){
    let graphData2: any[] = [];
    if(this.corridors !== undefined){
      this.corridors.forEach(corridor => {
        let lineData2 = this.lineData2.filter(data => data['corridor'] === corridor);
        let trace = {
          name: corridor,
          x: lineData2.map(value => new Date(value[this.line2.x])),
          y: lineData2.map(value => value[this.line2.y]),
          text: lineData2.map(value => value[this.line2.text]),
          hovertemplate: this.line2.hoverTemplate,
          mode: 'lines',
          line: {
            color: this.defaultColor
          }
        };

        graphData2.push(trace);
      });
    }

    this.lineGraph2.data = graphData2;
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

    this.lineGraph1.data
    .filter(item => item.name === name)
    .map(dataItem => {
      dataItem.line.color = this.selectColor;
      return dataItem;
    });

    this.lineGraph2.data
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

    this.lineGraph1.data.map(dataItem => {
      dataItem.line.color = this.defaultColor;
      return dataItem;
    });

    this.lineGraph2.data.map(dataItem => {
      dataItem.line.color = this.defaultColor;
      return dataItem;
    });
  }
}
