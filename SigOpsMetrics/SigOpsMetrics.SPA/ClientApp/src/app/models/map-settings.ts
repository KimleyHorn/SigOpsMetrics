import { Injectable } from "@angular/core";
import { Colors } from "./colors";
import { Metrics } from "./metrics";

@Injectable()
export class MapSettings {
  color: Colors = new Colors();

  mapSource = 'main';
  mapInterval = 'mo';
  mapLevel = 'sig';
  mapStart = '2021-03-01';
  mapEnd = '2021-03-02';

  //vehicles per day
  //vpMapField: string = "";
  //vpMapMetrics: Metrics = { measure: "", label: "Vehicles per day", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  vpMapRanges: number[][] = [[0, 50000],[50000,100000],[100000,150000]];
  vpLegendLabels: string[] = ["0-50,000","50,000-100,000","100,000-150,000"];
  vpLegendColors: string[] = [this.color.lightTeal,this.color.blue,this.color.darkBlue];

  //ped actuations
  //pedaMapField: string = "";
  //pedaMapMetrics: Metrics = { measure: "", label: "Ped Actuations per day", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  pedaMapRanges: number[][] = [[0, 1000],[1000,2000],[2000,3000],[3000,4000],[4000,5000]];
  pedaLegendLabels: string[] = ["0-1,000","1,000-2,000","2,000-3,000","3,000-4,000","4,000-5,000"];
  pedaLegendColors: string[] = [this.color.lightTeal,this.color.teal,this.color.blue,this.color.darkBlue,this.color.purple];

  //ped delay
  //peddMapField: string = "";
  //peddMapMetrics: Metrics = { measure: "", label: "Ped Delay", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  peddMapRanges: number[][] = [[0,50],[50,100],[100,150],[150,200]];
  peddLegendLabels: string[] = ["0-50","50-100","100-150","150-200"];
  peddLegendColors: string[] = [this.color.green,this.color.yellow,this.color.redOrange,this.color.red];

  //throughput
  tpMapField: string = "vph";
  tpMapMetrics: Metrics = new Metrics({ measure: "tp", field: "vph", label: "Throughput", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd });
  tpMapRanges: number[][] = [[0, 5000],[5000,10000],[10000,15000],[15000,20000],[20000,25000]];
  tpLegendLabels: string[] = ["0-5,000","5,000-10,000","10,000-15,000","15,000-20,000","20,000-25,000"];
  tpLegendColors: string[] = [this.color.lightTeal,this.color.teal,this.color.blue,this.color.darkBlue,this.color.purple];
  tpMapSettings = {
    metrics: this.tpMapMetrics,
    ranges: this.tpMapRanges,
    legendLabels: this.tpLegendLabels,
    legendColors: this.tpLegendColors,
  };

  //arrivals on green
  aogdMapField: string = "aog";
  aogdMapMetrics: Metrics = { measure: "aogd", field: "aog", label: "Arrivals on Green", formatType: "percent", formatDecimals: 2, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  aogdMapRanges: number[][] = [[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  aogdLegendLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  aogdLegendColors: string[] = [this.color.red,this.color.redOrange,this.color.yellow,this.color.greenYellow,this.color.green];
  aogdMapSettings = {
    metrics: this.aogdMapMetrics,
    ranges: this.aogdMapRanges,
    legendLabels: this.aogdLegendLabels,
    legendColors: this.aogdLegendColors,
  };

  //progression ratio
  prdMapField: string = "pr";
  prdMapMetrics: Metrics = { measure: "prd", field: "pr", label: "Progression Ration", formatDecimals: 1, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  prdMapRanges: number[][] = [[0,1],[1,2],[2,3],[3,4],[4,5],[5,6],[6,7]];
  prdLegendLabels: string[] = ["0-1","1-2","2-3","3-4","4-5","5-6","6-7"];
  prdLegendColors: string[] = [this.color.red,this.color.redOrange,this.color.orange,this.color.yellow,this.color.yellowGreen,this.color.greenYellow,this.color.green];
  prdMapSettings = {
    metrics: this.prdMapMetrics,
    ranges: this.prdMapRanges,
    legendLabels: this.prdLegendLabels,
    legendColors: this.prdLegendColors,
  };

  //queue spillback
  qsdMapField: string = "qs_freq";
  qsdMapMetrics: Metrics = { measure: "qsd", field: "qs_freq", label: "Queue Spillback", formatType: "percent", formatDecimals: 1, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  qsdMapRanges: number[][] = [[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  qsdLegendLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  qsdLegendColors: string[] = [this.color.green,this.color.greenYellow,this.color.yellow,this.color.redOrange,this.color.red];
  qsdMapSettings = {
    metrics: this.qsdMapMetrics,
    ranges: this.qsdMapRanges,
    legendLabels: this.qsdLegendLabels,
    legendColors: this.qsdLegendColors,
  };

  //peak split failures
  psfMapField: string = "sf_freq";
  psfMapMetrics: Metrics = { measure: "sfd", field: "sf_freq", label: "Peak Split Failures", formatDecimals: 2, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  psfMapRanges: number[][] = [[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  psfLegendLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  psfLegendColors: string[] = [this.color.green,this.color.greenYellow,this.color.yellow,this.color.redOrange,this.color.red];
  psfMapSettings = {
    metrics: this.psfMapMetrics,
    ranges: this.psfMapRanges,
    legendLabels: this.psfLegendLabels,
    legendColors: this.psfLegendColors,
  };

  //off-peak split failures
  osfMapField: string = "sf_freq";
  osfMapMetrics: Metrics = { measure: "sfo", field: "sf_freq", label: "Off-Peak Split Failures", formatDecimals: 2, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  osfMapRanges: number[][] = [[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  osfLegendLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  osfLegendColors: string[] = [this.color.green,this.color.greenYellow,this.color.yellow,this.color.redOrange,this.color.red];
  osfMapSettings = {
    metrics: this.osfMapMetrics,
    ranges: this.osfMapRanges,
    legendLabels: this.osfLegendLabels,
    legendColors: this.osfLegendColors,
  };

  //ped pushbutton availability
  //pedpbMapField: string = "";
  //pedpbMapMetrics: Metrics = { measure: "", label: "Ped Pushbutton Availability", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  pedpbMapRanges: number[][] = [[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  pedpbLegendLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  pedpbLegendColors: string[] = [this.color.red,this.color.redOrange,this.color.yellow,this.color.greenYellow,this.color.green];

  //communication uptime
  //commMapField: string = "";
  //commMapMetrics: Metrics = { measure: "", label: "Communication Uptime", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  commMapRanges: number[][] = [[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  commLegendLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  commLegendColors: string[] = [this.color.red,this.color.redOrange,this.color.yellow,this.color.greenYellow,this.color.green];

  //detector uptime
  //detMapField: string = "";
  //detMapMetrics: Metrics = { measure: "", label: "Detector Uptime", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  detMapRanges: number[][] = [[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  detLegendLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  detLegendColors: string[] = [this.color.red,this.color.redOrange,this.color.yellow,this.color.greenYellow,this.color.green];
}
