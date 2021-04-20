import { Component, Input, OnInit } from '@angular/core';
import { Metrics } from 'src/app/models/metrics';
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
  private _metricData;
  private _signals;

  public mapGraph: any;

  constructor(private _metricsService: MetricsService, 
    private _signalsService: SignalsService) { }

  ngOnInit(): void {
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

    this._metricsService.getMetrics(this.metrics).subscribe(response => {
      this._metricData = response;
 
      this.createMarkers();
    });

    this._signalsService.getData().subscribe((data) => {
      this._signals = data;
      
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


      let data = [
        {
          type: "scattermapbox",
          lat: joinedData.filter(signal => this._filterGreen(signal)).map(signal => signal.latitude),
          lon: joinedData.filter(signal => this._filterGreen(signal)).map(signal => signal.longitude),
          text: joinedData.filter(signal => this._filterGreen(signal)).map(signal => {
            return this._generateText(signal);
          }),
          marker: { 
            color: 'green',
            size: 6 
          },
          hovertemplate: '%{text}' +
            '<extra></extra>'
        },
        {
          type: "scattermapbox",
          lat: joinedData.filter(signal => this._filterYellow(signal)).map(signal => signal.latitude),
          lon: joinedData.filter(signal => this._filterYellow(signal)).map(signal => signal.longitude),
          text: joinedData.filter(signal => this._filterYellow(signal)).map(signal => {
            return this._generateText(signal);
          }),
          marker: { 
            color: 'yellow',
            size: 6 
          },
          hovertemplate: '%{text}' +
            '<extra></extra>'
        },
        {
          type: "scattermapbox",
          lat: joinedData.filter(signal => this._filterOrange(signal)).map(signal => signal.latitude),
          lon: joinedData.filter(signal => this._filterOrange(signal)).map(signal => signal.longitude),
          text: joinedData.filter(signal => this._filterOrange(signal)).map(signal => {
            return this._generateText(signal);
          }),
          marker: { 
            color: 'orange',
            size: 6 
          },
          hovertemplate: '%{text}' +
            '<extra></extra>'
        },
        {
          type: "scattermapbox",
          lat: joinedData.filter(signal => this._filterRedOrange(signal)).map(signal => signal.latitude),
          lon: joinedData.filter(signal => this._filterRedOrange(signal)).map(signal => signal.longitude),
          text: joinedData.filter(signal => this._filterRedOrange(signal)).map(signal => {
            return this._generateText(signal);
          }),
          marker: { 
            color: 'redorange',
            size: 6 
          },
          hovertemplate: '%{text}' +
            '<extra></extra>'
        },
        {
          type: "scattermapbox",
          lat: joinedData.filter(signal => this._filterRed(signal)).map(signal => signal.latitude),
          lon: joinedData.filter(signal => this._filterRed(signal)).map(signal => signal.longitude),
          text: joinedData.filter(signal => this._filterRed(signal)).map(signal => {
            return this._generateText(signal);
          }),
          marker: { 
            color: 'red',
            size: 6 
          },
          hovertemplate: '%{text}' +
            '<extra></extra>'
        },
      ];
      this.mapGraph.data = data;
    }
  }

  private _filterGreen(signal){
    if((this.metricField === "vph" && signal[this.metricField] < 5000)
      || (this.metricField === "qs_freq" && signal[this.metricField] < 0.2)
      || (this.metricField === "aog" && signal[this.metricField] < 0.2))
      return true;

    return false;
  }

  private _filterYellow(signal){
    if(this.metricField === "vph" && signal[this.metricField] >= 5000 && signal[this.metricField] < 10000
      || (this.metricField === "qs_freq" && signal[this.metricField] >= 0.2 && signal[this.metricField] < 0.4)
      || (this.metricField === "aog" && signal[this.metricField] >= 0.2 && signal[this.metricField] < 0.4))
      return true;
    
    return false;
  }

  private _filterOrange(signal){
    if((this.metricField === "vph" && signal[this.metricField] >= 10000 && signal[this.metricField] < 15000) 
      || (this.metricField === "qs_freq" && signal[this.metricField] >= 0.4 && signal[this.metricField] < 0.6)
      || (this.metricField === "aog" && signal[this.metricField] >= 0.4 && signal[this.metricField] < 0.6))
      return true;

    return false;
  }

  private _filterRedOrange(signal){
    if((this.metricField === "vph" && signal[this.metricField] >= 15000 && signal[this.metricField] < 20000)
      || (this.metricField === "qs_freq" && signal[this.metricField] >= 0.6 && signal[this.metricField] < 0.8)
      || (this.metricField === "aog" && signal[this.metricField] >= 0.6 && signal[this.metricField] < 0.8))
      return true;

    return false;
  }

  private _filterRed(signal){
    if((this.metricField === "vph" && signal[this.metricField] >= 20000)
      || (this.metricField === "qs_freq" && signal[this.metricField] >= 0.8)
      || (this.metricField === "aog" && signal[this.metricField] >= 0.8))
      return true;

    return false;
  }

  private _generateText(signal){
    let sigText = "<b>Signal: " + signal.signalID + "</b> | " + signal.mainStreetName + " @ " + signal.sideStreetName;
    let metricText = "<br><b>" + this.metricField + ": " + signal[this.metricField] + "</b>";
    return sigText + metricText;
  }
 
}
