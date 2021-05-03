import { Injectable } from "@angular/core";
import { Metrics } from "./metrics";

@Injectable()
export class MapSettings {
  mapStart = '2021-03-01';
  mapEnd = '2021-03-02';

  tpMapField: string = "vph";
  tpMapMetrics: Metrics = { measure: "tp", level: "sig", start: this.mapStart, end: this.mapEnd };
  tpMapLabels: string[] = ["0-5,000","5,000-10,000","10,000-15,000","15,000-20,000","Over 20,000"];

  aogdMapField: string = "aog";
  aogdMapMetrics: Metrics = { measure: "aogd", level: "sig", start: this.mapStart, end: this.mapEnd };
  aogdMapLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","Over 0.8"];

  prdMapField: string = "pr";
  prdMapMetrics: Metrics = { measure: "prd", level: "sig", start: this.mapStart, end: this.mapEnd };
  prdMapLabels: string[] = ["0-1","1-2","2-3","3-4","Over 4"];

  qsdMapField: string = "qs_freq";
  qsdMapMetrics: Metrics = { measure: "qsd", level: "sig", start: this.mapStart, end: this.mapEnd };
  qsdMapLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","Over 0.8"];

  sfMapField: string = "sf_freq";
  sfMapMetrics: Metrics = { measure: "sfo", level: "sig", start: this.mapStart, end: this.mapEnd };
  sfMapLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","Over 0.8"];
}
