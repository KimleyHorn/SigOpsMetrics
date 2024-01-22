import { Component, Input, OnInit, SimpleChanges } from '@angular/core';
import { from, Subscription } from 'rxjs';
import { Metrics } from 'src/app/models/metrics';
import { FilterService } from 'src/app/services/filter.service';
import { FormatService } from 'src/app/services/format.service';
import { MetricsService } from 'src/app/services/metrics.service';
import { SignalsService } from 'src/app/services/signals.service';
import { environment } from 'src/environments/environment';
import { DatePipe } from '@angular/common';
import { first, max, min } from 'rxjs/operators';

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
  private _filter;
  private isFiltering: boolean = true;

  public mapGraph: any;

  constructor(private _metricsService: MetricsService,
    private _signalsService: SignalsService,
    private _filterService: FilterService,
    private _formatService: FormatService,
    private _datePipe: DatePipe) {
      this.mapGraph = {
        data: [],
        layout: {
          xaxis: {
            zeroline: false
          },
          yaxis: {
            zeroline: false
          },
        },
        config: {
          displayModeBar: false
        }
      }
  }

  //format the date to 1900-12-31
  private _generateDate(day: number){
    let dt = new Date();
    dt.setDate(day);

    return this._datePipe.transform(dt, 'yyyy-MM-dd');
  }

  ngOnInit(): void {
    this.mapSettings.metrics.start = this._generateDate(1);
    this.mapSettings.metrics.end = this._generateDate(2);

    this._signalsService.getData().pipe(first()).subscribe(data => {
      this._signals = data.filter(signal => signal['latitude'] !== 0 && signal['longitude'] !== 0);
      this._loadMapData();
    });

    this._filterSubscription = this._filterService.filters.subscribe(filter => {
      this._filter = filter;
      this._loadMapData();
    })
  }

  ngOnChanges(changes: SimpleChanges){
    this._loadMapData();
  }

  //get the data for the map
  private _loadMapData(){
    if(this._filter !== undefined){
      this._metricsService.filterSignalMetrics(this.mapSettings.metrics, this._filter).pipe(first()).subscribe(response =>{
        this._metricData = response;
        this.createMarkers();
      });
    }
  }

  //clear subscribed events
  ngOnDestroy(): void {
    this._filterSubscription.unsubscribe();
  }

  //calculate average
  private _average(data, field){
    //dataitems
    let d = data.map(dataItem => dataItem[field]);
    //reduce to get the sum
    let r = d.reduce((a,b) => a + b);
    //total number of items
    let t = d.length;
    //the average
    let a = r / t;
    return a;
  }

  //caculate the zoom
  private _zoom(data){
    if (data.length === 1) return 12;
    let latitudes = data.map(dataItem => dataItem['latitude']);
    let longitudes = data.map(dataItem => dataItem['longitude']);
    let minLat = Math.min(...latitudes);
    let maxLat = Math.max(...latitudes);
    let minLng = Math.min(...longitudes);
    let maxLng = Math.max(...longitudes);

    let widthY = maxLat - minLat;
    let widthX = maxLng - minLng;

    let zoomY = -1.446 * Math.log(widthY) + 8.2753; //7.2753
    let zoomX = -1.415 * Math.log(widthX) + 9.7068; //8.7068

    return Math.min(zoomY, zoomX);
  }

  //create the marker points for the map
  createMarkers(){
    if (this._metricData !== undefined && this._signals !== undefined && this._filter !== undefined) {
      let joinedData = this._signals.map(signal => {
        let newSignal = signal;
        let dataItem = this._metricData.filter(md => md["label"] === signal.signalID)[0]
        if (dataItem !== undefined) {
          newSignal[this.mapSettings.metrics.field] = dataItem["avg"];
          return newSignal;
        }
      });
      joinedData = joinedData.filter(item => item);
      let data = [];

      for (let index = 0; index < this.mapSettings.ranges.length; index++) {
        const range = this.mapSettings.ranges[index];
        let markerSignals = joinedData.filter(signal => {
          if(signal[this.mapSettings.metrics.field] >= range[0] && signal[this.mapSettings.metrics.field] <= range[1])
            return true;
          return false;
        });

        data.push({
          type: "scattermapbox",
          lat: this._mapData(markerSignals.map(signal => signal.latitude)),
          lon: this._mapData(markerSignals.map(signal => signal.longitude)),
          text: markerSignals.map(signal => {return this._generateText(signal)}),
          marker : {
            color: this.mapSettings.legendColors[index],
            size: 6
          },
          name: this.mapSettings.legendLabels[index],
          showlegend: true,
          hovertemplate: '%{text}' + '<extra></extra>'
        });
      }

      this.mapGraph.data = data;

      let centerLat = this._average(joinedData, 'latitude');
      let centerLon = this._average(joinedData, 'longitude');
      let zoom = this._zoom(joinedData);
      let layout = {
        dragmode: "zoom",
        mapbox: {
          style: "carto-positron",
          center: {
            //lat: environment.mapCenterLat,
            //lon: environment.mapCenterLon
            lat: centerLat,
            lon: centerLon
          },
          zoom: zoom
        },
        margin: { r: 0, t: 0, b: 0, l: 0 },
        xaxis: {
          zeroline: false,
        },
        yaxis: {
          zeroline: false,
        },
        legend: {
          x: 1,
          xanchor: 'right',
          y: 0.9,
        }
      };
      this.mapGraph.layout = layout;
    }
  }

  //return null if no data is available
  private _mapData(data){
    if(data.length <= 0){
      return [null];
    }

    return data;
  }

  //create the text for the map plot tooltips
  private _generateText(signal){
    let sigText = "<b>Signal: " + signal.signalID + "</b> | " + signal.mainStreetName + " @ " + signal.sideStreetName;
    let value = "";

    if (signal[this.mapSettings.metrics.field] === -1){
      value = "Unavailable";
    }
    else{
      if(this.mapSettings.metrics.formatType === "percent"){
        value = this._formatService.formatPercent(signal[this.mapSettings.metrics.field], this.mapSettings.metrics.formatDecimals);
      }
      else{
        value = this._formatService.formatNumber(signal[this.mapSettings.metrics.field], this.mapSettings.metrics.formatDecimals);
      }
    }

    let metricText = "<br><b>" + this.mapSettings.metrics.label + ": " + value + "</b>";
    return sigText + metricText;
  }

}
