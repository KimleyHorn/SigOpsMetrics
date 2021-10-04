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

  ngOnInit(): void {
    //initialize graph
    this.graphConfig = {
      data: [],
      layout: {
        showlegend: false,
        xaxis: {
          tickangle: 90
        },
        yaxis:{
          automargin: true,
          tickmode: "linear",
          range: [0, 10]
        },
        margin:{
          t:25
        },
        hovermode: 'closest'
      }
    }

    //sets up filter sub so when user changes filter the current graph refreshes
    this._filterSubscription = this._filterService.filters.subscribe(filter => {
      this.graphFilter = filter; 
      this._getMetricData();
      this._loadGraph();
    });

  }
  
  ngOnChanges(){}

  ngOnDestroy(): void {
    this._filterSubscription.unsubscribe();
  }

  private _getMetricData() {
    this._metricsService.filterMetrics(this.metrics, this.graphFilter).subscribe(data => {
      this.graphData = data;
      this._loadGraph();
    });
  }

  private _loadGraph(){
    //load the data to the graph if the filter and metric data have been returned
    if(this.graphFilter !== undefined && this.graphData !== undefined){
      let data: any[] = [];

      //group the data - seems like we only care about task_source/task_subtype/task_type and oustanding?
      let reFilteredData = [];
        let type;
        for (const [key] of Object.entries(this.graphData[0])) {
          if (key.includes('task_')) {
            type = key;
          }
        }

        this.graphData.forEach(element => {
          let found = false;
          var newItem = {};     
          for (let i=0; i<reFilteredData.length;i++) {
            if (reFilteredData[i][type] == element[type]) {
              reFilteredData[i].outstanding += element.outstanding;
              found = true;
              break;
            } 
          }       
          if (!found) {
            newItem[type] = element[type];
            newItem["outstanding"] = element.outstanding;
            reFilteredData.push(newItem);
          }               
        })
              
      //sort the data
      let sortedData = reFilteredData.sort((n1, n2) => n1[this.graph.x] - n2[this.graph.x]);

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
      this.graphConfig.layout.yaxis.nticks = sortedData.length;

      //forces starting position of Y axis to top of range
      if (sortedData.length > 10) {
        this.graphConfig.layout.yaxis.range = [sortedData.length - 10,sortedData.length];
      } else {
        this.graphConfig.layout.yaxis.range = [0,sortedData.length];
      }
      this.graphConfig.data = data;
    } 
  }
}
