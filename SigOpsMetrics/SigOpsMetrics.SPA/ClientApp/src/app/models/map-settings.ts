import { Injectable } from "@angular/core";
import { Metrics } from "./metrics";

@Injectable()
export class MapSettings {
  mapSource = 'main';
  mapInterval = 'mo';
  mapLevel = 'sig';
  mapStart = '2021-03-01';
  mapEnd = '2021-03-02';

  //vehicles per day
  //vpMapField: string = "";
  //vpMapMetrics: Metrics = { measure: "", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  vpMapRanges: number[][] = [[0, 50000],[50000,100000],[100000,150000]];
  vpLegendLabels: string[] = ["0-50,000","50,000-100,000","100,000-150,000"];
  vpLegendColors: string[] = ["#66d99e","#0070ed","#6600cc"];

  //ped actuations
  //pedaMapField: string = "";
  //pedaMapMetrics: Metrics = { measure: "", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  pedaMapRanges: number[][] = [[0, 1000],[1000,2000],[2000,3000],[3000,4000],[4000,5000]];
  pedaLegendLabels: string[] = ["0-1,000","1,000-2,000","2,000-3,000","3,000-4,000","4,000-5,000"];
  pedaLegendColors: string[] = ["#66d99e","#00a698","#0070ed","#0c4f98","#6600cc"];

  //ped delay
  //peddMapField: string = "";
  //peddMapMetrics: Metrics = { measure: "", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  peddMapRanges: number[][] = [[0,50],[50,100],[100,150],[150,200]];
  peddLegendLabels: string[] = ["0-50","50-100","100-150","150-200"];
  peddLegendColors: string[] = ["#04c360","#ffd600","#ff6b00","#ff0000"];

  //throughput
  tpMapField: string = "vph";
  tpMapMetrics: Metrics = { measure: "tp", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  tpMapRanges: number[][] = [[0, 5000],[5000,10000],[10000,15000],[15000,20000],[20000,25000]];
  tpLegendLabels: string[] = ["0-5,000","5,000-10,000","10,000-15,000","15,000-20,000","20,000-25,000"];
  tpLegendColors: string[] = ["#66d99e","#00a698","#0070ed","#0c4f98","#6600cc"];

  //arrivals on green
  aogdMapField: string = "aog";
  aogdMapMetrics: Metrics = { measure: "aogd", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  aogdMapRanges: number[][] = [[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  aogdLegendLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  aogdLegendColors: string[] = ["#04c360","#80cc2f","#ffd600","#ff6b00","#ff0000"];

  //progression ratio
  prdMapField: string = "pr";
  prdMapMetrics: Metrics = { measure: "prd", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  prdMapRanges: number[][] = [[0,1],[1,2],[2,3],[3,4],[4,5],[5,6],[6,7]];
  prdLegendLabels: string[] = ["0-1","1-2","2-3","3-4","4-5","5-6","6-7"];
  prdLegendColors: string[] = ["#04c360","#80cc2f","#cdd312","#ffd600","#ffac00","#ff5600","#ff0000"];

  //queue spillback
  qsdMapField: string = "qs_freq";
  qsdMapMetrics: Metrics = { measure: "qsd", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  qsdMapRanges: number[][] = [[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  qsdLegendLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  qsdLegendColors: string[] = ["#04c360","#80cc2f","#ffd600","#ff6b00","#ff0000"];

  //peak split failures
  psfMapField: string = "sf_freq";
  psfMapMetrics: Metrics = { measure: "sfd", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  psfMapRanges: number[][] = [[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  psfLegendLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  psfLegendColors: string[] = ["#04c360","#80cc2f","#ffd600","#ff6b00","#ff0000"];

  //off-peak split failures
  osfMapField: string = "sf_freq";
  osfMapMetrics: Metrics = { measure: "sfo", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  osfMapRanges: number[][] = [[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  osfLegendLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  osfLegendColors: string[] = ["#04c360","#80cc2f","#ffd600","#ff6b00","#ff0000"];

  //ped pushbutton availability
  //pedpbMapField: string = "";
  //pedpbMapMetrics: Metrics = { measure: "", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  pedpbMapRanges: number[][] = [[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  pedpbLegendLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  pedpbLegendColors: string[] = ["#04c360","#80cc2f","#ffd600","#ff6b00","#ff0000"];

  //communication uptime
  //commMapField: string = "";
  //commMapMetrics: Metrics = { measure: "", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  commMapRanges: number[][] = [[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  commLegendLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  commLegendColors: string[] = ["#04c360","#80cc2f","#ffd600","#ff6b00","#ff0000"];

  //detector uptime
  //detMapField: string = "";
  //detMapMetrics: Metrics = { measure: "", source: this.mapSource, interval: this.mapInterval, level: this.mapLevel, start: this.mapStart, end: this.mapEnd };
  detMapRanges: number[][] = [[0.0,0.2],[0.2,0.4],[0.4,0.6],[0.6,0.8],[0.8,1]];
  detLegendLabels: string[] = ["0.0-0.2","0.2-0.4","0.4-0.6","0.6-0.8","0.8-1.0"];
  detLegendColors: string[] = ["#04c360","#80cc2f","#ffd600","#ff6b00","#ff0000"];
}
