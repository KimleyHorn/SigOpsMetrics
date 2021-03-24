import { Component, OnInit } from "@angular/core";
import { SignalInfo } from "src/app/models/signal-info";
import { SignalsService } from "../../services/signals.service";
import { environment } from "../../../environments/environment";

@Component({
  selector: "app-map",
  templateUrl: "./map.component.html",
  styleUrls: ["./map.component.css"],
})
export class MapComponent implements OnInit {
  markers = [];
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
  };

  constructor(private signalsService: SignalsService) {}

  ngOnInit() {
    this.center = {
      lat: environment.mapCenterLat,
      lng: environment.mapCenterLon,
    };
    this.addMarkers();
  }

  addMarkers() {
    this.signalsService.getData().subscribe((data) => {
      data.forEach((element) => {
        this.addMarker(element);
      });
    });
  }

  addMarker(signal: SignalInfo) {
    this.markers.push({
      position: {
        lat: signal.latitude,
        lng: signal.longitude,
      },
      title: signal.mainStreetName + " @ " + signal.sideStreetName,
      options: {
        //icon: svgMarker
        icon: "../../assets/images/greencircle.png",
        //animation: google.maps.Animation.BOUNCE
      },
    });
  }
}
