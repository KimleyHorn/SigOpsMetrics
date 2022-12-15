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
  private _filterErrorStateSubscription: Subscription;
  private _color = new Colors();

  @Input() title: string = "";
  corridors: any;
  @Input() data: any;
  @Input() averageData: any;

  @Input() line: Graph;
  lineGraph: any;
  lineData: any;

  //bar graph input values
  @Input() bar: Graph;
  barGraph: any;
  barData: any;

  bothData: any[] = [];
  bothLayout: any;

  filter: Filter;

  defaultColor: string = this._color.gray;
  primaryColor: string = this._color.blue;
  secondaryColor: string = this._color.darkGray;

  filterErrorState: number;

  constructor(private _filterService: FilterService) {}

  ngOnInit(): void {
    this.bothLayout = {
      grid: {rows: 1, columns: 2, pattern: 'independent'},
      showlegend: false,
      xaxis: {
        title: this.bar.title,
        tickangle: 90,
        domain: [0,.25]
      },
      yaxis:{
        automargin: true,
        type: 'category'
      },
      margin:{
        t:25
      },
      xaxis2: {
        title: this.line.title,
        domain: [.3,1]
      },
      yaxis2:{
        automargin: true,
      },
      hovermode: 'closest'
    };
  }

  ngOnChanges(changes: SimpleChanges){
    this.bothData = [];
    this.barGraph = {
      data: [],
      layout: {}
    };

    this.lineGraph = {
      data: [],
      layout: {}
    };

    this._filterErrorStateSubscription = this._filterService.errorState.subscribe(errorState => {
      this.filterErrorState = errorState;
    })
    //when the filters are loaded or changed
    this._filterSubscription = this._filterService.filters.subscribe(filter => {
      if(this.data !== undefined){
        this.lineData = this.data;
        let cors;
        if (this.filter.zone_Group === 'All') {
          cors = new Set(this.data.filter(value => value['actualZoneGroup'] !== null).map(data => data['actualZoneGroup']));
        } else {
          cors = new Set(this.data.filter(value => value['corridor'] !== null).map(data => data['corridor']));
        }
        this.corridors = Array.from(cors);

        this.barData = this.averageData;
      }

      //set the local filter variable
      this.filter = filter;

      //set the color for the trace
      this._selectTrace(filter.zone_Group);
    });
  }

  ngOnDestroy(): void {
    this._filterSubscription.unsubscribe();
    this._filterErrorStateSubscription.unsubscribe();
  }

  //Left graph is averaged/grouped by corridor/signal
  private _loadBarGraph(){
    let graphData: any[] = [];
    this.bothData = [];
    if(this.corridors !== undefined){
      let sortedData = this.barData.sort((n1, n2) => n1.avg - n2.avg);
      sortedData.forEach(sortItem => {
        let trace = {
          name: sortItem.label,
          x: [sortItem.avg],
          y: [sortItem.label],
          orientation: 'h',
          type: 'bar',
          hovertemplate: this.bar.hoverTemplate,
          marker: {
            color: this.defaultColor
          }
        };
        graphData.push(trace);
        this.bothData.push(trace);
      });
    }
    this.barGraph.data = graphData;
    this.barGraph.data = this.filterErrorState == 2 ? [] : graphData;
  }

  // Right graph is averaged/grouped per month.
  private _loadLineGraph(){
    this.updateLineXAxis();
    //this.updateLineYAxis();

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
          },
          xaxis: 'x2',
          yaxis: 'y2'
        };
        this.bothData.push(trace);
        graphData.push(trace);
      });
    }

    this.lineGraph.data = this.filterErrorState == 2 ? [] : graphData;
  }

  updateLineXAxis() {
    if (this.title === "Percent Health") {
      this.line.x = "month";
    } else {
      switch (this.filter.timePeriod) {
        case 0: //qhr
          this.line.x = "timeperiod";
          break;
        case 1: //hr
          this.line.x = "hour";
          break;
        case 2: //dy
          this.line.x = "date";
          break;
        case 3: //wk
          this.line.x = "date";
          break;
        case 4: //mo
          this.line.x = "month";
          break;
        case 5: //qu
          this.line.x = "quarter";
          break;
      }
    }
  }

  updateLineYAxis() {
    //weird case for volume because the columns keep changing
    if ((this.line.y == 'vpd' || this.line.y == 'vol') && this.filter.timePeriod == 1) {
      this.line.y = 'vph';
    } else if ((this.line.y == 'vpd' || this.line.y == 'vph') && this.filter.timePeriod == 0) {
      this.line.y = 'vol';
    }
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
    this._resetColor(this.bothData);

    //update the trace colors for each graph
    this._changeColor(this.bothData, name);
  }

  private _changeColor(data, name){
    data.filter(item => item.name === name || item.name === this.filter.zone_Group)
    .map(dataItem => {
      if(dataItem.name === name && name === this.filter.zone_Group){
        //if the filered zone group is the selected trace, set the trace to the secondary color
        if (dataItem.hasOwnProperty('line')){
          dataItem['line'].color = this.primaryColor;
        }
        else{
          dataItem['marker'].color = this.primaryColor;
        }
      }else if(dataItem.name === this.filter.zone_Group){
        //if the filered zone group is not the selected trace, set the trace to the secondary color
        if (dataItem.hasOwnProperty('line')){
          dataItem['line'].color = this.secondaryColor;
        }
        else{
          dataItem['marker'].color = this.secondaryColor;
        }
      }else{
        //all other selected traces will be the primary color
        if (dataItem.hasOwnProperty('line')){
          dataItem['line'].color = this.primaryColor;
        }
        else{
          dataItem['marker'].color = this.primaryColor;
        }
      }
      return dataItem;
    });
  }

  private _resetColor(data){
    data.map(dataItem => {
      if (dataItem.hasOwnProperty('line')){
        dataItem['line'].color = this.defaultColor;
      }
      else{
        dataItem['marker'].color = this.defaultColor;
      }
      return dataItem;
    });
  }
}
