import { isNgTemplate } from '@angular/compiler';
import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { Subscription } from 'rxjs';
import { Colors } from 'src/app/models/colors';
import { Filter } from 'src/app/models/filter';
import { Graph } from 'src/app/models/graph';
import { FilterService } from 'src/app/services/filter.service';

@Component({
  selector: 'app-bar-line-graph',
  templateUrl: './bar-line-graph.component.html',
  styleUrls: ['./bar-line-graph.component.css']
})
export class BarLineGraphComponent implements OnInit, OnChanges {
  private _filterSubscription: Subscription;
  private _color = new Colors();

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

  filter: Filter;

  defaultColor: string = this._color.gray;
  primaryColor: string = this._color.blue;
  secondaryColor: string = this._color.darkGray;

  constructor(private _filterService: FilterService) {}

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

    //when the filters are loaded or changed
    this._filterSubscription = this._filterService.filters.subscribe(filter => {
      if(this.data !== undefined){
        this.lineData = this.data;
        let splitMonth = filter.month.split('/');

        let month = parseInt(splitMonth[0]) - 1;
        let year = parseInt(splitMonth[1]);

        this.barData = this.data.filter(dataItem => {
          let dataMonth = new Date(dataItem['month']).getMonth();
          let dataYear = new Date(dataItem['month']).getFullYear();
          return dataMonth === month && dataYear === year
        });

        let cors = new Set(this.data.filter(value => value['corridor'] !== null).map(data => data['corridor']));
        this.corridors = Array.from(cors);
      }

      //set the local filter variable
      this.filter = filter;
      //set the color for the trace
      this._selectTrace(filter.zone_Group);
    });
  }

  ngOnDestroy(): void {
    this._filterSubscription.unsubscribe();
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

  //triggered when a graph trace is selected
  graphClicked(e){
    var name = e.points[0].data.name;
    this._selectTrace(name);
  }

  private _selectTrace(name: string){
    if(this.corridors !== undefined){
      //reorder corridors to bring the selected trace to the front
      let cor = this.corridors.filter(x => x === name)[0];
      this.corridors.push(this.corridors.splice(this.corridors.indexOf(cor), 1)[0]);
      this._loadBarGraph();
      this._loadLineGraph();
    }

    //reset the trace colors to the default
    this._resetColor(this.barGraph.data, "marker");
    this._resetColor(this.lineGraph.data, "line");

    //update the trace colors for each graph
    this._changeColor(this.barGraph.data, "marker", name);
    this._changeColor(this.lineGraph.data, "line", name);
  }

  //adjust the color for the selected trace
  private _changeColor(data, traceType, name){
    data.filter(item => item.name === name || item.name === this.filter.zone_Group)
    .map(dataItem => {
      if(dataItem.name === name && name === this.filter.zone_Group){
        //if the filered zone group is the selected trace, set the trace to the secondary color
        dataItem[traceType].color = this.primaryColor;
      }else if(dataItem.name === this.filter.zone_Group){
        //if the filered zone group is not the selected trace, set the trace to the secondary color
        dataItem[traceType].color = this.secondaryColor;
      }else{
        //all other selected traces will be the primary color
        dataItem[traceType].color = this.primaryColor;
      }
      return dataItem;
    });
  }

  //reset the color of all traces to default
  private _resetColor(data, traceType){
    data.map(dataItem => {
      dataItem[traceType].color = this.defaultColor;
      return dataItem;
    });
  }
}
