import { Component, Input, OnInit, SimpleChanges } from '@angular/core';
import { Subscription } from 'rxjs';
import { Metrics } from 'src/app/models/metrics';
import { FilterService } from 'src/app/services/filter.service';
import { FormatService } from 'src/app/services/format.service';
import { MetricsService } from 'src/app/services/metrics.service';
import { SignalsService } from 'src/app/services/signals.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-scatter-map',
  templateUrl: './scatter-map.component.html',
  styleUrls: ['./scatter-map.component.css']
})
export class ScatterMapComponent implements OnInit {
  private _filterSubscription: Subscription;

  @Input() mapSettings;
  private _metricData;
  private _signals;
  private _corridors;

  public mapGraph: any;

  constructor(private _metricsService: MetricsService,
    private _signalsService: SignalsService,
    private _filterService: FilterService,
    private _formatService: FormatService) {
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
    this._metricsService.getMetrics(this.mapSettings.metrics).subscribe(response => {
      this._metricData = response;
      this.createMarkers();
    });

    this._signalsService.getData().subscribe(data => {
      this._signals = data;
      this.createMarkers();
    });

    this._filterService.corridors.subscribe(data => {
      this._corridors = data;
      this.createMarkers();
    });

    this._filterSubscription = this._filterService.filters.subscribe(() =>{
      this.createMarkers();
    });
  }

  ngOnDestroy(): void {
    this._filterSubscription.unsubscribe();
  }

  createMarkers(){
    if(this._metricData !== undefined && this._signals !== undefined && this._corridors !== undefined){
      let joinedData = this._signals.map(signal =>{
        let newSignal = signal;
        let dataItem = this._metricData.filter(md => md["corridor"] === signal.signalID || md["corridor"] === signal.corridor)[0]
        if(dataItem !== undefined){
          newSignal[this.mapSettings.metrics.field] = dataItem[this.mapSettings.metrics.field];
        }else{
          newSignal[this.mapSettings.metrics.field] = 0;
        }
        return newSignal;
      });

      let filteredData = this._filterService.filterData(joinedData, this._corridors);

      let data = [];
      for (let index = 0; index < this.mapSettings.ranges.length; index++) {
        const range = this.mapSettings.ranges[index];
        let markerSignals = filteredData.filter(signal => {
          if(signal[this.mapSettings.metrics.field] >= range[0] && signal[this.mapSettings.metrics.field] < range[1])
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
            color: this.mapSettings.legendColors[index],
            size: 6
          },
          name: this.mapSettings.legendLabels[index],
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
    let value = "";

    if(this.mapSettings.metrics.formatType === "percent"){
      value = this._formatService.formatPercent(signal[this.mapSettings.metrics.field], this.mapSettings.metrics.formatDecimals);
    }else{
      value = this._formatService.formatNumber(signal[this.mapSettings.metrics.field], this.mapSettings.metrics.formatDecimals);
    }

    let metricText = "<br><b>" + this.mapSettings.metrics.label + ": " + value + "</b>";
    return sigText + metricText;
  }

}
