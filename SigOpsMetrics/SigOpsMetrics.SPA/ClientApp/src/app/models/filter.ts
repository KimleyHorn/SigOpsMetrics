export class Filter {
  month: string;
  dateRange: number = 2;
  customStart: Date = null;
  customEnd: Date = null;
  daysOfWeek: string[] = null;
  startTime: Date = null;
  endTime: Date = null;
  zone_Group: string = 'RTOP1';
  //zone_Group: string = 'All RTOP';
  zone: string = null; //aka district
  agency: string = null;
  //zoneGroup: string // aka signal group
  county: string = null;
  city: string = null;
  corridor: string = null;
  timePeriod: number = 4;

  constructor() {
    let _dt = new Date();
    this.month = (_dt.getMonth() + 1) + "/" + _dt.getFullYear();

    // this.customStart = new Date(_dt.getFullYear() - 1, _dt.getMonth(), _dt.getDate());
    // this.customEnd = _dt;

      // let _dt = new Date();
      // let newMonth = _dt.getMonth() - 1;
      // if(newMonth < 0){
      //     newMonth += 12;
      //     _dt.setFullYear(_dt.getFullYear() - 1);
      // }
      // _dt.setMonth(newMonth);

      // this.month = (_dt.getMonth() + 1) + "/" + _dt.getFullYear();
  }

}
