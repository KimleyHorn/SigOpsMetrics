export class Filter {
  month: string;
  dataRange: string;
  customStart: Date;
  customEnd: Date;
  daysOfWeek: string[];
  startTime: Date;
  endTime: Date;
  dataAggregation: string;
  zone_Group: string = 'All RTOP';
  zone: string; //aka district
  agency: string;
  //zoneGroup: string // aka signal group
  county: string;
  city: string;
  corridor: string;
  timePeriod: number;

  constructor(){
    let _dt = new Date();
    this.month = (_dt.getMonth() + 1) + "/" + _dt.getFullYear();

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
