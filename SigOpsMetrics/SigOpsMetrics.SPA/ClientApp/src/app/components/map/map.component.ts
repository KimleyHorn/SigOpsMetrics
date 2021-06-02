import { Component, Input, OnInit, ViewChild } from "@angular/core";
import { SignalInfo } from "src/app/models/signal-info";
import { SignalsService } from "../../services/signals.service";
import { environment } from "../../../environments/environment";
import { MetricsService } from "src/app/services/metrics.service";
import { Metrics } from "src/app/models/metrics";
import { MapInfoWindow, MapMarker } from "@angular/google-maps";

@Component({
  selector: "app-map",
  templateUrl: "./map.component.html",
  styleUrls: ["./map.component.css"],
})
export class MapComponent implements OnInit {
  @ViewChild(MapInfoWindow, {static: false}) infoWindow: MapInfoWindow;
  @Input() height: string;
  @Input() metrics: Metrics;
  markers = [];
  metricData: any;
  signals: any;
  infoContent: any;
  defaultLat: number;
  defaultLon: number;
  zoom = 12;
  center: google.maps.LatLngLiteral;
  options: google.maps.MapOptions = {
    mapTypeId: "hybrid",
    zoomControl: false,
    scrollwheel: true,
    disableDoubleClickZoom: true,
    maxZoom: 15,
    minZoom: 8,
    fullscreenControlOptions: {
      position: google.maps.ControlPosition.BOTTOM_RIGHT
    }
  };

  constructor(private signalsService: SignalsService,
    private _metricsService: MetricsService) {}

  ngOnInit() {
    this.center = {
      lat: environment.mapCenterLat,
      lng: environment.mapCenterLon,
    };
    
    if(this.metrics !== undefined){
      this._metricsService.getMetrics(this.metrics).subscribe(response => {
        this.metricData = response;
   
        this.createMarkers();
      });
    }

    this.signalsService.getData().subscribe((data) => {
      this.signals = data;
      this.createMarkers();
    });
  }

  createMarkers(){
    if(this.signals !== undefined && this.metricData !== undefined){
      this.signals.forEach((element) => {
        this.addMarker(element);
      });
    }
  }

  addMarker(signal: SignalInfo) {
    let dataItem = this.metricData.filter(md => md["corridor"] === signal.signalID)[0];

    this.markers.push({
      position: {
        lat: signal.latitude,
        lng: signal.longitude,
      },
      title: signal.mainStreetName + " @ " + signal.sideStreetName,
      options: {
        icon: this._selectIcon(dataItem),
      },
      info: this._createWindowInfo(signal, dataItem)
    });
  }

  openInfo(marker: MapMarker, content){
    this.infoContent = content;
    this.infoWindow.open(marker);
  }

  private _createWindowInfo(signal: SignalInfo, dataItem){
    let sig = "<strong>Signal: " + signal.signalID + "</strong> | " + signal.mainStreetName + " @ " + signal.sideStreetName;
    let metric = "";
    if(dataItem !== undefined){
      metric += "<br/><strong>vph:</strong> " + dataItem["vph"];
    }
    return sig + metric;
  }

  private _selectIcon(dataItem){
    let val = 0;
    if(dataItem !== undefined){
      val = dataItem["vph"];
    }
    let icon = "";

    switch (true) {
      case val > 20000:
        icon = "../../assets/images/redcircle.png";
        break;
      case val > 15000:
        icon = "../../assets/images/redcircle.png";
        break;
      case val > 10000:
        icon = "../../assets/images/yellowcircle.png";
        break;
      case val > 5000:
        icon = "../../assets/images/yellowcircle.png";
        break;
      default:
        icon = "../../assets/images/greencircle.png";
        break;
    }

    return icon;
  }
}
