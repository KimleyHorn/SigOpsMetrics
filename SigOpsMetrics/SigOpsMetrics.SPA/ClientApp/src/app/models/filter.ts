import { min } from "rxjs/operators";

export class Filter {
  //month: string;
  dateRange: number = 4;
  timePeriod: number = 4;
  customStart: Date = null;
  customEnd: Date = null;
  daysOfWeek: string[] = null;
  startTime: Date = null;
  endTime: Date = null;
  zone_Group: string = 'Central Metro';
  //zone_Group: string = 'All RTOP';
  zone: string = null; //aka district
  agency: string = null;
  //zoneGroup: string // aka signal group
  county: string = null;
  city: string = null;
  corridor: string = null;
  signalId: string = "";
  constructor() {
    // let _dt = new Date();
    // this.month = (_dt.getMonth() + 1) + "/" + _dt.getFullYear();
  }

}