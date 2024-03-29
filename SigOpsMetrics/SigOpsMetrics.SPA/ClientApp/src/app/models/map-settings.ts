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
  vpMapRanges: number[][] = [[-1,-1],[0, 50000],[50000,100000],[100000,150000]];
  vpLegendLabels: string[] = ["Unavailable","0-50,000","50,000-100,000","100,000-150,000"];
  vpLegendColors: string[] = [this.color.gray,this.color.lightTeal,this.color.blue,this.color.darkBlue];

  //ped actuations
  //pedaMapField: string = "";
  //pedaMapMetrics: Metrics = { measure: "", label: "Ped Actuations per day", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel };
  pedaMapRanges: number[][] = [[-1,-1],[0, 1000],[1000,2000],[2000,3000],[3000,4000],[4000,5000]];
  pedaLegendLabels: string[] = ["Unavailable","0-1,000","1,000-2,000","2,000-3,000","3,000-4,000","4,000-5,000"];
  pedaLegendColors: string[] = [this.color.gray,this.color.lightTeal,this.color.teal,this.color.blue,this.color.darkBlue,this.color.purple];

  //ped delay
  //peddMapField: string = "";
  //peddMapMetrics: Metrics = { measure: "", label: "Ped Delay", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel };
  peddMapRanges: number[][] = [[-1,-1],[0,50],[50,100],[100,150],[150,200]];
  peddLegendLabels: string[] = ["Unavailable","0-50","50-100","100-150","150-200"];
  peddLegendColors: string[] = [this.color.gray,this.color.green,this.color.yellow,this.color.redOrange,this.color.red];

  //throughput
  tpMapField: string = "vph";
  tpMapMetrics: Metrics = new Metrics({ measure: "tp", field: "vph", label: "Throughput", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
  tpMapRanges: number[][] = [[-1,-1],[0, 2000],[2001,4000],[4001,6000],[6001,8000],[8001,100000]];
  tpLegendLabels: string[] = ["Unavailable","0 - 2000","2,001 - 4,000","4,001 - 6,000","6,001 - 8,000","8,001+"];
  tpLegendColors: string[] = [this.color.gray, this.color.lightTeal,this.color.teal,this.color.blue,this.color.darkBlue,this.color.purple];
  tpMapSettings = {
    metrics: this.tpMapMetrics,
    ranges: this.tpMapRanges,
    legendLabels: this.tpLegendLabels,
    legendColors: this.tpLegendColors,
  };

  //arrivals on green
  aogdMapField: string = "aog";
  aogdMapMetrics: Metrics = new Metrics({ measure: "aogd", field: "aog", label: "Arrivals on Green", formatType: "percent", formatDecimals: 1, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
  aogdMapRanges: number[][] = [[-1,-1],[0,0.2],[0.21,0.4],[0.41,0.6],[0.61,0.8],[0.8,1]];
  aogdLegendLabels: string[] = ["Unavailable","0% - 20%","21% - 40%","41% - 60%","61% - 80%","81% - 100%"];
  aogdLegendColors: string[] = [this.color.gray,this.color.purple,this.color.redOrange,this.color.yellow,this.color.greenYellow,this.color.green];
  aogdMapSettings = {
    metrics: this.aogdMapMetrics,
    ranges: this.aogdMapRanges,
    legendLabels: this.aogdLegendLabels,
    legendColors: this.aogdLegendColors,
  };

  //progression ratio
  prdMapField: string = "pr";
  prdMapMetrics: Metrics = new Metrics({ measure: "prd", field: "pr", label: "Progression Ratio", formatDecimals: 2, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
  prdMapRanges: number[][] = [[-1,-1],[0,0.4],[0.41,0.8],[0.81,1],[1.01,1.2],[1.2,10]];
  prdLegendLabels: string[] = ["Unavailable","0 - 0.4","0.41 - 0.8","0.81 - 1","1.01 - 1.2","1.2+"];
  prdLegendColors: string[] = [this.color.gray,this.color.red,this.color.redOrange,this.color.orange,this.color.yellow,this.color.yellowGreen];
  prdMapSettings = {
    metrics: this.prdMapMetrics,
    ranges: this.prdMapRanges,
    legendLabels: this.prdLegendLabels,
    legendColors: this.prdLegendColors,
  };

  //queue spillback
  qsdMapField: string = "qs_freq";
  qsdMapMetrics: Metrics = new Metrics({ measure: "qsd", field: "qs_freq", label: "Queue Spillback", formatType: "percent", formatDecimals: 1, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
  qsdMapRanges: number[][] = [[-1,-1],[0,0.2],[0.21,0.4],[0.41,0.6],[0.61,0.8],[0.81,1]];
  qsdLegendLabels: string[] = ["Unavailable","0% - 20%","20.01% - 40%","40.01% - 60%","60.01% - 80%","80.01% - 100%"];
  qsdLegendColors: string[] = [this.color.gray,this.color.green,this.color.greenYellow,this.color.yellow,this.color.redOrange,this.color.red];
  qsdMapSettings = {
    metrics: this.qsdMapMetrics,
    ranges: this.qsdMapRanges,
    legendLabels: this.qsdLegendLabels,
    legendColors: this.qsdLegendColors,
  };

  //peak split failures
  psfMapField: string = "sf_freq";
  psfMapMetrics: Metrics = new Metrics({ measure: "sfd", field: "sf_freq", label: "Peak Split Failures", formatType: "percent", formatDecimals: 1, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
  psfMapRanges: number[][] = [[-1,-1],[0,0.05],[0.051,0.1],[0.101,0.15],[0.151,0.2],[0.201,1]];
  psfLegendLabels: string[] = ["Unavailable","0% - 5%","5.1% - 10%","10.1% - 15%","15.1% - 20%","20.1%+"];
  psfLegendColors: string[] = [this.color.gray,this.color.green,this.color.greenYellow,this.color.yellow,this.color.redOrange,this.color.red];
  psfMapSettings = {
    metrics: this.psfMapMetrics,
    ranges: this.psfMapRanges,
    legendLabels: this.psfLegendLabels,
    legendColors: this.psfLegendColors,
  };

  //off-peak split failures
  osfMapField: string = "sf_freq";
  osfMapMetrics: Metrics = new Metrics({ measure: "sfo", field: "sf_freq", label: "Off-Peak Split Failures", formatType: "percent", formatDecimals: 1, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
  osfMapRanges: number[][] = [[-1,-1],[0,0.05],[0.051,0.1],[0.101,0.15],[0.151,0.2],[0.201,1]];
  osfLegendLabels: string[] = ["Unavailable","0% - 5%","5.1% - 10%","10.1% - 15%","15.1% - 20%","20.1%+"];
  osfLegendColors: string[] = [this.color.gray,this.color.green,this.color.greenYellow,this.color.yellow,this.color.redOrange,this.color.red];
  osfMapSettings = {
    metrics: this.osfMapMetrics,
    ranges: this.osfMapRanges,
    legendLabels: this.osfLegendLabels,
    legendColors: this.osfLegendColors,
  };

  //ped pushbutton availability
  //pedpbMapField: string = "";
  //pedpbMapMetrics: Metrics = { measure: "", label: "Ped Pushbutton Availability", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel };
  pedpbMapRanges: number[][] = [[-1,-1],[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  pedpbLegendLabels: string[] = ["Unavailable","0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  pedpbLegendColors: string[] = [this.color.gray,this.color.red,this.color.redOrange,this.color.yellow,this.color.greenYellow,this.color.green];

  //communication uptime
  //commMapField: string = "";
  //commMapMetrics: Metrics = { measure: "", label: "Communication Uptime", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel };
  commMapRanges: number[][] = [[-1,-1],[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  commLegendLabels: string[] = ["Unavailable","0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  commLegendColors: string[] = [this.color.gray,this.color.red,this.color.redOrange,this.color.yellow,this.color.greenYellow,this.color.green];

  //detector uptime
  //detMapField: string = "";
  //detMapMetrics: Metrics = { measure: "", label: "Detector Uptime", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel };
  detMapRanges: number[][] = [[-1,-1],[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  detLegendLabels: string[] = ["Unavailable","0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  detLegendColors: string[] = [this.color.gray,this.color.red,this.color.redOrange,this.color.yellow,this.color.greenYellow,this.color.green];

  //daily traffic volume
  dtvMapField: string = "vpd";
  dtvMapMetrics: Metrics = new Metrics({ measure: "vpd", field: "vpd", label: "Daily Traffic Volume", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
  dtvMapRanges: number[][] = [[-1,-1],[0,10000],[10001,20000],[20001,30000],[30001,40000],[40001,10000000]];
  dtvLegendLabels: string[] = ["Unavailable","0 - 10,000","10,001 - 20,000","20,001 - 30,000", "30,001 - 40,000", "40,001+"];
  dtvLegendColors: string[] = [this.color.gray,this.color.lightTeal, this.color.teal, this.color.blue, this.color.darkBlue, this.color.purple];
  dtvMapSettings = {
    metrics: this.dtvMapMetrics,
    ranges: this.dtvMapRanges,
    legendLabels: this.dtvLegendLabels,
    legendColors: this.dtvLegendColors,
  };
  //daily pedestrian pushbutton activity
  papdMapField: string = "papd";
  papdMapMetrics: Metrics = new Metrics({ measure: "papd", field: "papd", label: "Daily Pedestrian Pushbutton Activity", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
  papdMapRanges: number[][] = [[-1,-1],[0,100],[101,200],[201,300],[301,400],[400,5000]];
  papdLegendLabels: string[] = ["Unavailable","0 - 100","101 - 200","201 - 300","301 - 400","400+"];
  papdLegendColors: string[] = [this.color.gray,this.color.lightTeal,this.color.teal,this.color.blue,this.color.darkBlue,this.color.purple];
  papdMapSettings = {
    metrics: this.papdMapMetrics,
    ranges: this.papdMapRanges,
    legendLabels: this.papdLegendLabels,
    legendColors: this.papdLegendColors,
  };
  //detector uptime
  duMapField: string = "uptime";
  duMapMetrics: Metrics = new Metrics({ measure: "du", field: "uptime", label: "Detector Uptime", formatType: "percent", formatDecimals: 1, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
  duMapRanges: number[][] = [[-1,-1],[0,0.6],[0.61,0.8],[0.81,0.9],[0.91,0.95],[0.95,1]];
  duLegendLabels: string[] = ["Unavailable","0% - 60%","60.01% - 80%","80.01% - 90%","90.1% - 95%","95.1%+"];
  duLegendColors: string[] = [this.color.gray,this.color.red,this.color.redOrange,this.color.yellow,this.color.greenYellow,this.color.green];
  duMapSettings = {
    metrics: this.duMapMetrics,
    ranges: this.duMapRanges,
    legendLabels: this.duLegendLabels,
    legendColors: this.duLegendColors,
  };
  //pedestrian pushbutton uptime
  pauMapField: string = "uptime";
  pauMapMetrics: Metrics = new Metrics({ measure: "pau", field: "uptime", label: "Pedestrian Pushbutton Uptime", formatType: "percent", formatDecimals: 1, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
  pauMapRanges: number[][] = [[-1,-1],[0,0.6],[0.61,0.8],[0.81,0.9],[0.91,0.95],[0.95,1]];
  pauLegendLabels: string[] = ["Unavailable","0% - 60%","60.01% - 80%","80.01% - 90%","90.1% - 95%","95.1%+"];
  pauLegendColors: string[] = [this.color.gray,this.color.red,this.color.redOrange,this.color.yellow,this.color.greenYellow,this.color.green];
  pauMapSettings = {
    metrics: this.pauMapMetrics,
    ranges: this.pauMapRanges,
    legendLabels: this.pauLegendLabels,
    legendColors: this.pauLegendColors,
  };
  //cctv uptime
  cctvMapField: string = "uptime";
  cctvMapMetrics: Metrics = new Metrics({ measure: "cctv", field: "uptime", label: "CCTV Uptime", formatDecimals: 2, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
  cctvMapRanges: number[][] = [[-1,-1],[0,0.6],[0.61,0.8],[0.81,0.9],[0.91,0.95],[0.95,1]];
  cctvLegendLabels: string[] = ["Unavailable","0% - 60%","60.01% - 80%","80.01% - 90%","90.1% - 95%","95.1%+"];
  cctvLegendColors: string[] = [this.color.gray,this.color.red,this.color.redOrange,this.color.yellow,this.color.greenYellow,this.color.green];
  cctvMapSettings = {
    metrics: this.cctvMapMetrics,
    ranges: this.cctvMapRanges,
    legendLabels: this.cctvLegendLabels,
    legendColors: this.cctvLegendColors,
  };
  //communication uptime
  cuMapField: string = "uptime";
  cuMapMetrics: Metrics = new Metrics({ measure: "cu", field: "uptime", label: "Communication Uptime", formatType: "percent", formatDecimals: 1, source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, isMapMetrics: true });
  cuMapRanges: number[][] = [[-1,-1],[0,0.6],[0.61,0.8],[0.81,0.9],[0.91,0.95],[0.95,1]];
  cuLegendLabels: string[] = ["Unavailable","0% - 60%","60.01% - 80%","80.01% - 90%","90.1% - 95%","95.1%+"];
  cuLegendColors: string[] = [this.color.gray,this.color.red,this.color.redOrange,this.color.yellow,this.color.greenYellow,this.color.green];
  cuMapSettings = {
    metrics: this.cuMapMetrics,
    ranges: this.cuMapRanges,
    legendLabels: this.cuLegendLabels,
    legendColors: this.cuLegendColors,
  };
}
