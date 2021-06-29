import { Component, Input, OnInit, SimpleChanges } from '@angular/core';
import { Subscription } from 'rxjs';
import { Metrics } from 'src/app/models/metrics';
import { FilterService } from 'src/app/services/filter.service';
import { FormatService } from 'src/app/services/format.service';
import { MetricsService } from 'src/app/services/metrics.service';
import { SignalsService } from 'src/app/services/signals.service';
import { environment } from 'src/environments/environment';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-scatter-map',
  templateUrl: './scatter-map.component.html',
  styleUrls: ['./scatter-map.component.css']
})
export class ScatterMapComponent implements OnInit {
  private _metricSubscription: Subscription;
  private _signalSubscription: Subscription;
  private _corridorSubscription: Subscription;
  private _filterSubscription: Subscription;
  private _serviceSubscription: Subscription;

  @Input() mapSettings;
  private _metricData;
  private _signals;
  private _corridors;
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

  private _generateDate(day: number){
    let dt = new Date();
    dt.setDate(day);

    return this._datePipe.transform(dt, 'yyyy-MM-dd');
  }

  ngOnInit(): void {
    this.mapSettings.metrics.start = this._generateDate(1);
    this.mapSettings.metrics.end = this._generateDate(2);
  }

  ngOnChanges(changes: SimpleChanges){
    this._loadMapData();
  }

  private _loadMapData(){
    //TODO: create new method to return signals and not corridors
    this._metricSubscription = this._metricsService.getMetrics(this.mapSettings.metrics).subscribe(response => {
      this._metricData = response;
      this.createMarkers();
    });

    //TODO: REMOVE THIS!!! Will not be needed when there is filtered signal metric data
    this._signalSubscription = this._signalsService.getData().subscribe(data => {
      this._signals = data;
      this.createMarkers();
    });

    //TODO: REMOVE THIS!!! Will not be needed when there is filtered signal metric data
    this._corridorSubscription = this._filterService.corridors.subscribe(data => {
      this._corridors = data;
      this.createMarkers();
    });

    //TODO: REMOVE THIS!!! Will not be needed when there is filtered signal metric data
    this._filterSubscription = this._filterService.filters.subscribe(filter => {
      this._filter = filter;
      this.createMarkers();
    });

    this._serviceSubscription = this._filterService.isFiltering.subscribe(filtering => {
      this.isFiltering = filtering;
      this.createMarkers();
    });
  }

  ngOnDestroy(): void {
    this._metricSubscription.unsubscribe();
    //TODO: REMOVE THIS!!! Will not be needed when there is filtered signal metric data
    this._signalSubscription.unsubscribe();
    //TODO: REMOVE THIS!!! Will not be needed when there is filtered signal metric data
    this._corridorSubscription.unsubscribe();
    //TODO: REMOVE THIS!!! Will not be needed when there is filtered signal metric data
    this._filterSubscription.unsubscribe();
    this._serviceSubscription.unsubscribe();
  }

  createMarkers(){
    if(this._metricData !== undefined
      //TODO: REMOVE THIS!!! Will not be needed when there is filtered signal metric data
      && this._signals !== undefined
      //TODO: REMOVE THIS!!! Will not be needed when there is filtered signal metric data
      && this._corridors !== undefined
      //TODO: REMOVE THIS!!! Will not be needed when there is filtered signal metric data
      && this._filter !== undefined
      && !this.isFiltering)
    {
      //TODO: REMOVE THIS!!! To be replaced with filtered signal metric data
      //join the corridor metric data with the signals
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

      //TODO: REMOVE THIS!!! To be replaced with filtered signal metric data
      //filter out the appropriate data
      let filteredData = this._filterService.filterData(joinedData, this._corridors);

      let data = [];
      //create data ranges for markers
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
