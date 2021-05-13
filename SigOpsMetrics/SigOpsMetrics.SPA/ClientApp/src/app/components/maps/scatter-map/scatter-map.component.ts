import { Component, Input, OnInit, SimpleChanges } from '@angular/core';
import { Metrics } from 'src/app/models/metrics';
import { FilterService } from 'src/app/services/filter.service';
import { MetricsService } from 'src/app/services/metrics.service';
import { SignalsService } from 'src/app/services/signals.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-scatter-map',
  templateUrl: './scatter-map.component.html',
  styleUrls: ['./scatter-map.component.css']
})
export class ScatterMapComponent implements OnInit {
  @Input() metrics: Metrics;
  @Input() metricField: string = "vph";
  @Input() legendColors: string[] = ["green","yellow","orange","redorange","red"];
  @Input() legendLabels: string[] = ["trace 1","trace 2","trace 3","trace 4","trace 5"];
  @Input() mapRanges: number[][] = [];
  private _metricData;
  private _signals;

  public mapGraph: any;

  constructor(private _metricsService: MetricsService,
    private _signalsService: SignalsService,
    private _filterService: FilterService) {
      this.mapGraph = {
        data: [],
        layout: {
          dragmode: "zoom",
          mapbox: {
            style: "carto-positron",
            center: {
              lat: environment.mapCenterLat,
              lon: environment.mapCenterLon
            },
            zoom: 12
          },
          margin: { r: 0, t: 0, b: 0, l: 0 },
          xaxis: {
            zeroline: false
          },
          yaxis: {
            zeroline: false
          },
          legend: {
            x: 1,
            xanchor: 'right',
            y: 0.9,
          }
        }
      }
    }

  ngOnInit(): void {}

  ngOnChanges(changes: SimpleChanges){
    this._metricsService.getMetrics(this.metrics).subscribe(response => {
      this._metricData = response;

      this.createMarkers();
    });

    this._signalsService.getData().subscribe(data => {
      this._signals = data;

      this.createMarkers();
    });

    this._filterService.filters.subscribe(() =>{
      this.createMarkers();
    });
  }

  createMarkers(){
    if(this._metricData !== undefined && this._signals !== undefined){
      let joinedData = this._signals.map(signal =>{
        let newSignal = signal;
        let dataItem = this._metricData.filter(md => md["corridor"] === signal.signalID || md["corridor"] === signal.corridor)[0]
        if(dataItem !== undefined){
          newSignal[this.metricField] = dataItem[this.metricField];
        }else{
          newSignal[this.metricField] = 0;
        }
        return newSignal;
      });

      joinedData = this._filterService.filterData(joinedData);

      let data = [];

      for (let index = 0; index < this.mapRanges.length; index++) {
        const range = this.mapRanges[index];

        let markerSignals = joinedData.filter(signal => {
          if(signal[this.metricField] >= range[0] && signal[this.metricField] < range[1])
            return true;

          return false;
        });

        data.push({
          type: "scattermapbox",
          lat: this._mapData(markerSignals.map(signal => signal.latitude)),
          lon: this._mapData(markerSignals.map(signal => signal.longitude)),
          text: markerSignals.map(signal => {
            return this._generateText(signal);
          }),
          marker: {
            color: this.legendColors[index],
            size: 6
          },
          name: this.legendLabels[index],
          showlegend: true,
          hovertemplate: '%{text}' +
            '<extra></extra>'
        });
      }

      this.mapGraph.data = data;
    }
  }

  private _mapData(data){
    if(data.length <= 0){
      return [null];
    }

    return data;
  }

  private _generateText(signal){
    let sigText = "<b>Signal: " + signal.signalID + "</b> | " + signal.mainStreetName + " @ " + signal.sideStreetName;
    let metricText = "<br><b>" + this.metrics.label + ": " + signal[this.metricField] + "</b>";
    return sigText + metricText;
  }

}
