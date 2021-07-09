export class Filter {
  //month: string;
  dateRange: number = 4;
  customStart: Date = null;
  customEnd: Date = null;
  daysOfWeek: string[] = null;
  startTime: Date = null;
  endTime: Date = null;
  zone_Group: string = 'RTOP2';
  //zone_Group: string = 'All RTOP';
  zone: string = null; //aka district
  agency: string = null;
  //zoneGroup: string // aka signal group
  county: string = null;
  city: string = null;
  corridor: string = null;
  timePeriod: number = 4;

  constructor() {
    // let _dt = new Date();
    // this.month = (_dt.getMonth() + 1) + "/" + _dt.getFullYear();
  }

}
