import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { Graph } from 'src/app/models/graph';

@Component({
  selector: 'app-bar-line-graph',
  templateUrl: './bar-line-graph.component.html',
  styleUrls: ['./bar-line-graph.component.css']
})
export class BarLineGraphComponent implements OnInit, OnChanges {
  private _currentMonth = new Date().getMonth();

  @Input() title: string = "";
  corridors: any;
  @Input() data: any;

  @Input() line: Graph;
  lineGraph: any;
  lineData: any;

  //bar graph input values
  @Input() bar: Graph;
  barGraph: any;
  barData: any;

  defaultColor: string = '#A9A9A9';
  selectColor: string = 'red';

  constructor() {}

  ngOnInit(): void {}

  ngOnChanges(changes: SimpleChanges){
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
        margin:{
          t:25
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
        margin:{
          t:25
        },
        hovermode: 'closest'
      }
    };

    if(this.data !== undefined){
      this._loadGraphs();
    }
  }
  
  private _loadGraphs(){
    this.lineData = this.data;
    //TODO: adjusted this filter to be based on the selected month
    this.barData = this.data.filter(dataItem => new Date(dataItem['month']).getMonth() === this._currentMonth);
    this.corridors = new Set(this.data.filter(value => value['corridor'] !== null).map(data => data['corridor']));

    this._loadBarGraph();
    this._loadLineGraph();
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

    // let index = this.lineGraph.data.findIndex(item => item.name === name);
    // var lineItem = this.lineGraph.data[index];
    // this.lineGraph.data.splice(index, 1);
    // this.lineGraph.data.splice(0, 0, lineItem);
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
