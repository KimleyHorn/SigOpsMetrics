import { Component, Input, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { Filter } from 'src/app/models/filter';
import { Graph } from 'src/app/models/graph';
import { Metrics } from 'src/app/models/metrics';
import { FilterService } from 'src/app/services/filter.service';
import { MetricsService } from 'src/app/services/metrics.service';

@Component({
  selector: 'app-tickets-graph',
  templateUrl: './tickets-graph.component.html',
  styleUrls: ['./tickets-graph.component.css']
})
export class TicketsGraphComponent implements OnInit {
  private _filterSubscription: Subscription;

  @Input() metrics: Metrics = new Metrics();
  @Input() graph: Graph = new Graph();
  graphConfig: any;
  graphData: any;
  graphFilter: Filter;

  defaultColor: string = "red";

  constructor(private _metricsService: MetricsService, private _filterService: FilterService) { }

  ngOnInit(): void {}

  ngOnChanges(){
    //set the initial configuration of the graph
    this.graphConfig = {
      data: [],
      layout: {
        showlegend: false,
        xaxis: {
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
    }

    //load the filters
    this._filterSubscription = this._filterService.filters.subscribe(filter => {
      this.graphFilter = filter;

      this._loadGraph();
    });

    //get the graph data
    this._metricsService.getMetrics(this.metrics).subscribe(data => {
      this.graphData = data;

      this._loadGraph();
    });
  }

  ngOnDestroy(): void {
    this._filterSubscription.unsubscribe();
  }

  private _loadGraph(){
    //load the data to the graph if the filter and metric data have been returned
    if(this.graphFilter !== undefined && this.graphData !== undefined){
      let data: any[] = [];
      
      //filter the data based on the set filter
      //let filteredData = this.graphData.filter(dataItem => dataItem.zone_Group === this.graphFilter.zone_Group);
      let allRegions = ['Cobb County','District 1','District 2','District 3','District 4','District 5','District 6','District 7','Ramp Meters','RTOP1','RTOP2']
      let filteredData = this.graphData.filter(dataItem => dataItem.zone_Group === this.graphFilter.zone_Group || (this.graphFilter.zone_Group === 'All' && allRegions.includes(dataItem.zone_Group)));

      //sort the data
      let sortedData = filteredData.sort((n1, n2) => n1[this.graph.x] - n2[this.graph.x]);

      //create the traces for the graph
      sortedData.forEach(sortItem => {
        let trace = {
          name: sortItem[this.graph.y],
          x: [sortItem[this.graph.x]],
          y: [sortItem[this.graph.y]],
          orientation: 'h',
          type: 'bar',
          hovertemplate: this.graph.hoverTemplate,
          marker: {
            color: this.defaultColor
          }
        };

        data.push(trace);
      });

      //add all the traces to the graph
      this.graphConfig.data = data;
    }
  }
}
