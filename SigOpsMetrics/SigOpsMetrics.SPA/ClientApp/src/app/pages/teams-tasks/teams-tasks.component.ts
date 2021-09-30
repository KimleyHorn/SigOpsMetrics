import { core } from '@angular/compiler';
import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { filter } from 'rxjs/operators';
import { Colors } from 'src/app/models/colors';
import { Filter } from 'src/app/models/filter';
import { Graph } from 'src/app/models/graph';
import { Metrics } from 'src/app/models/metrics';
import { FilterService } from 'src/app/services/filter.service';
import { FormatService } from 'src/app/services/format.service';
import { MetricsService } from 'src/app/services/metrics.service';

@Component({
  selector: 'app-teams-tasks',
  templateUrl: './teams-tasks.component.html',
  styleUrls: ['./teams-tasks.component.css']
})
export class TeamsTasksComponent implements OnInit {
  color: Colors = new Colors();
  bigGraphConfig: any;
  bigGraphData: any;

  private _hoverTemplate: string = '%{y}:' +
  '<b>%{x}</b>' +
  '<extra></extra>';

  metricsSource: Metrics = new Metrics();
  graphSource: Graph = {
    x: 'outstanding',
    y: 'task_Source',
    hoverTemplate: this._hoverTemplate,
  };
  metricsType: Metrics =  new Metrics();
  graphType: Graph = {
    x: 'outstanding',
    y: 'task_Type',
    hoverTemplate: this._hoverTemplate,
  };
  metricsSubtype: Metrics =  new Metrics();
  graphSubtype: Graph = {
    x: 'outstanding',
    y: 'task_Subtype',
    hoverTemplate: this._hoverTemplate,
  };

  metrics: any[] = [
    {
      metricLabel: "Tasks Reported",
      measure: "reported",
      trace: {
        type: "bar",
        orientation: "v",
        color: this.color.red,
      },
      metricValue: null,
      metricChange: null
    },
    {
      metricLabel: "Tasks Resolved",
      measure: "resolved",
      trace: {
        type: "bar",
        orientation: "v",
        color: this.color.green
      },
      metricValue: null,
      metricChange: null
    },
    {
      metricLabel: "Tasks Outstanding",
      measure: "outstanding",
      trace: {
        type: "lines",
        color: this.color.yellow
      },
      metricValue: null,
      metricChange: null
    },
    { metricLabel: "Tasks Over 45 Days", measure: "over45", metricValue: null, metricChange: null },
    //{ metricLabel: "Resolve" },
  ]

  private _dt: Date = new Date();
  private _filter: Filter = new Filter();

  tabs: any[];

  constructor(
    private _metricsService: MetricsService,
    private _filterSerivce: FilterService,
    private _formatService: FormatService,
    private titleService:Title) {

    //get the current month
    let metricDateStartDate = (this._dt.getMonth()) + '/' + (this._dt.getFullYear() - 1);
    let metricDateEndDate = (this._dt.getMonth()) + '/' + (this._dt.getFullYear());
    this.metricsSource.measure = "tsou";
    this.metricsSource.start = metricDateStartDate;
    this.metricsSource.end = metricDateEndDate;

    this.metricsType.measure = "ttyp";
    this.metricsType.start = metricDateStartDate;
    this.metricsType.end = metricDateEndDate;

    this.metricsSubtype.measure = "tsub";
    this.metricsSubtype.start = metricDateStartDate;
    this.metricsSubtype.end = metricDateEndDate;

    this.tabs = [
      { label: "SOURCE", metrics: this.metricsSource, graph: this.graphSource, level: "cor", interval: "mo", measure: "tsou" },
      { label: "TYPE", metrics: this.metricsType, graph: this.graphType, level: "cor", interval: "mo", measure: "ttyp" },
      { label: "SUBTYPE", metrics: this.metricsSubtype, graph: this.graphSubtype, level: "cor", interval: "mo", measure: "tsub" },
    ];
   }

  ngOnInit(): void {
    this.titleService.setTitle("SigOpsMetrics - TEAMSTasks")

    //create the layout for the big graph
    this.bigGraphConfig = {
      data: [],
      layout: {
        showlegend: true,
        legend: {
          "orientation": "h",
          x: 0,
          y: 1.1
        },
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

    this._filterSerivce.filters.subscribe(filter => {
      this._filter = filter;
      this._loadMetricCards();
    });
  }

  private _loadMetricCards(){
    this.bigGraphConfig.data = [];

    for (let index = 0; index < this.metrics.length; index++) {
      const item = this.metrics[index];

      let metric = new Metrics();
      metric.measure = item.measure;
      this._metricsService.filterMetrics(metric, this._filter).subscribe(data => {
        let metricData = [];
        // TODO - make this work with other filter levels - currently only works with zone_group
        if (this._filter.zone_Group) {
          data.forEach(element => {
              element.corridor = this._filter.zone_Group;
              element.zone_Group = this._filter.zone_Group;
              element.counter = 0;
              let found = false;
              for (let i=0; i<metricData.length;i++) {
                if (metricData[i].month == element.month) {
                  metricData[i].counter += 1;
                  metricData[i].reported += element.reported;
                  metricData[i].over45 += element.over45;
                  metricData[i].resolved += element.resolved;
                  metricData[i].outstanding += element.outstanding;
                  metricData[i].delta += element.delta;
                  found = true;
                  break;
                } 
              }       
              if (!found) {
                element.counter = 1;
                metricData.push(element);
              }               
          })
          metricData.forEach(x => {
            // delta is an average so we fix it here? not sure this is correct.
            x.delta /= x.counter;
          })
        } else {
          metricData = data.filter(di =>
            di["corridor"] === this._filter.zone_Group || di["corridor"] === this._filter.corridor || this._filter.signalId != ''
          );
        }

        let metricItem = metricData[metricData.length-1];
        item.metricValue = metricItem[item.measure];
        item.metricChange = "(" + this._formatService.formatPercent(metricItem["delta"],2) + ")";

        if(item.trace !== undefined){
          let trace = {
            index: index,
            name: item.metricLabel,
            x: metricData.map(data => data["month"]),
            y: metricData.map(data => data[item.measure]),
            orientation: item.trace.orientation,
            type: item.trace.type,
            hovertemplate: item.metricLabel +
            '<b>: %{y}</b>' +
            '<extra></extra>',
            marker: {
              color: item.trace.color
            }
          };

          this.bigGraphConfig.data.push(trace);

          this.bigGraphConfig.data = this.bigGraphConfig.data.sort((n1, n2) => n1["index"] - n2["index"]);
        }
      });
    }
  }

}
