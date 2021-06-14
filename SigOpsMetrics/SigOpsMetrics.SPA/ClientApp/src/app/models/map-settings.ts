import { Injectable } from "@angular/core";
import { Colors } from "./colors";
import { Metrics } from "./metrics";

@Injectable()
export class MapSettings {
  color: Colors = new Colors();

  mapSource = 'main';
  mapInterval = 'mo';
  mapLevel = 'sig';

  //vehicles per day
  //vpMapField: string = "";
  //vpMapMetrics: Metrics = { measure: "", label: "Vehicles per day", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel };
  vpMapRanges: number[][] = [[0, 50000],[50000,100000],[100000,150000]];
  vpLegendLabels: string[] = ["0-50,000","50,000-100,000","100,000-150,000"];
  vpLegendColors: string[] = [this.color.lightTeal,this.color.blue,this.color.darkBlue];

  //ped actuations
  //pedaMapField: string = "";
  //pedaMapMetrics: Metrics = { measure: "", label: "Ped Actuations per day", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel };
  pedaMapRanges: number[][] = [[0, 1000],[1000,2000],[2000,3000],[3000,4000],[4000,5000]];
  pedaLegendLabels: string[] = ["0-1,000","1,000-2,000","2,000-3,000","3,000-4,000","4,000-5,000"];
  pedaLegendColors: string[] = [this.color.lightTeal,this.color.teal,this.color.blue,this.color.darkBlue,this.color.purple];

  //ped delay
  //peddMapField: string = "";
  //peddMapMetrics: Metrics = { measure: "", label: "Ped Delay", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel };
  peddMapRanges: number[][] = [[0,50],[50,100],[100,150],[150,200]];
  peddLegendLabels: string[] = ["0-50","50-100","100-150","150-200"];
  peddLegendColors: string[] = [this.color.green,this.color.yellow,this.color.redOrange,this.color.red];

  //throughput
  tpMapField: string = "vph";
  tpMapMetrics: Metrics = new Metrics({ measure: "tp", field: "vph", label: "Throughput", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
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
  aogdMapMetrics: Metrics = new Metrics({ measure: "aogd", field: "aog", label: "Arrivals on Green", formatType: "percent", formatDecimals: 2, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
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
  prdMapMetrics: Metrics = new Metrics({ measure: "prd", field: "pr", label: "Progression Ration", formatDecimals: 1, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
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
  qsdMapMetrics: Metrics = new Metrics({ measure: "qsd", field: "qs_freq", label: "Queue Spillback", formatType: "percent", formatDecimals: 1, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
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
  psfMapMetrics: Metrics = new Metrics({ measure: "sfd", field: "sf_freq", label: "Peak Split Failures", formatDecimals: 2, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
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
  osfMapMetrics: Metrics = new Metrics({ measure: "sfo", field: "sf_freq", label: "Off-Peak Split Failures", formatDecimals: 2, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
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
  //pedpbMapMetrics: Metrics = { measure: "", label: "Ped Pushbutton Availability", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel };
  pedpbMapRanges: number[][] = [[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  pedpbLegendLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  pedpbLegendColors: string[] = [this.color.red,this.color.redOrange,this.color.yellow,this.color.greenYellow,this.color.green];

  //communication uptime
  //commMapField: string = "";
  //commMapMetrics: Metrics = { measure: "", label: "Communication Uptime", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel };
  commMapRanges: number[][] = [[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  commLegendLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  commLegendColors: string[] = [this.color.red,this.color.redOrange,this.color.yellow,this.color.greenYellow,this.color.green];

  //detector uptime
  //detMapField: string = "";
  //detMapMetrics: Metrics = { measure: "", label: "Detector Uptime", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel };
  detMapRanges: number[][] = [[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  detLegendLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  detLegendColors: string[] = [this.color.red,this.color.redOrange,this.color.yellow,this.color.greenYellow,this.color.green];

  //daily traffic volume
  dtvMapField: string = "vpd";
  dtvMapMetrics: Metrics = new Metrics({ measure: "vpd", field: "vpd", label: "Daily Traffic Volume", formatDecimals: 2, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
  dtvMapRanges: number[][] = [[0,50000],[50000,100000],[100000,150000]];
  dtvLegendLabels: string[] = ["0-50,000","50,000-100,000","100,000-150,000"];
  dtvLegendColors: string[] = [this.color.lightTeal, this.color.blue, this.color.purple];
  dtvMapSettings = {
    metrics: this.dtvMapMetrics,
    ranges: this.dtvMapRanges,
    legendLabels: this.dtvLegendLabels,
    legendColors: this.dtvLegendColors,
  };
  //daily pedestrian pushbutton activity
  papdMapField: string = "papd";
  papdMapMetrics: Metrics = new Metrics({ measure: "papd", field: "papd", label: "Daily Pedestrian Pushbutton Activity", formatDecimals: 2, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
  papdMapRanges: number[][] = [[0,1000],[1000,2000],[2000,3000],[3000,4000],[4000,5000]];
  papdLegendLabels: string[] = ["0-1,000","1,000-2,000","2,000-3,000","3,000-4,000","4,000-5,000"];
  papdLegendColors: string[] = [this.color.lightTeal,this.color.teal,this.color.blue,this.color.darkBlue,this.color.purple];
  papdMapSettings = {
    metrics: this.papdMapMetrics,
    ranges: this.papdMapRanges,
    legendLabels: this.papdLegendLabels,
    legendColors: this.papdLegendColors,
  };
  //detector uptime
  duMapField: string = "uptime";
  duMapMetrics: Metrics = new Metrics({ measure: "du", field: "uptime", label: "Detector Uptime", formatType: "percent", formatDecimals: 2, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
  duMapRanges: number[][] = [[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  duLegendLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  duLegendColors: string[] = [this.color.red,this.color.redOrange,this.color.yellow,this.color.greenYellow,this.color.greenYellow];
  duMapSettings = {
    metrics: this.duMapMetrics,
    ranges: this.duMapRanges,
    legendLabels: this.duLegendLabels,
    legendColors: this.duLegendColors,
  };
  //pedestrian pushbutton uptime
  pauMapField: string = "uptime";
  pauMapMetrics: Metrics = new Metrics({ measure: "pau", field: "uptime", label: "Pedestrian Pushbutton Uptime", formatDecimals: 2, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
  pauMapRanges: number[][] = [[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  pauLegendLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  pauLegendColors: string[] = [this.color.green,this.color.greenYellow,this.color.yellow,this.color.redOrange,this.color.red];
  pauMapSettings = {
    metrics: this.pauMapMetrics,
    ranges: this.pauMapRanges,
    legendLabels: this.pauLegendLabels,
    legendColors: this.pauLegendColors,
  };
  //cctv uptime
  cctvMapField: string = "uptime";
  cctvMapMetrics: Metrics = new Metrics({ measure: "cctv", field: "uptime", label: "CCTV Uptime", formatDecimals: 2, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
  cctvMapRanges: number[][] = [[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  cctvLegendLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  cctvLegendColors: string[] = [this.color.green,this.color.greenYellow,this.color.yellow,this.color.redOrange,this.color.red];
  cctvMapSettings = {
    metrics: this.cctvMapMetrics,
    ranges: this.cctvMapRanges,
    legendLabels: this.cctvLegendLabels,
    legendColors: this.cctvLegendColors,
  };
  //communication uptime
  cuMapField: string = "uptime";
  cuMapMetrics: Metrics = new Metrics({ measure: "cu", field: "uptime", label: "Communication Uptime", formatDecimals: 2, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
  cuMapRanges: number[][] = [[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  cuLegendLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  cuLegendColors: string[] = [this.color.green,this.color.greenYellow,this.color.yellow,this.color.redOrange,this.color.red];
  cuMapSettings = {
    metrics: this.cuMapMetrics,
    ranges: this.cuMapRanges,
    legendLabels: this.cuLegendLabels,
    legendColors: this.cuLegendColors,
  };
}
