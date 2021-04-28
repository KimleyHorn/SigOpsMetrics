import { DatePipe } from "@angular/common";

export class Filter {
    zone_Group: string = 'All RTOP';
    corridor: string
    month: string

    //zoneGroup: string // aka signal group
    zone: string //aka district
    agency: string
    county: string
    city: string
    timePeriod: number
    customStart: Date
    customEnd: Date
    startTime: Date
    endTime: Date

    constructor(){
        let _dt = new Date();
        let newMonth = _dt.getMonth() - 1;
        if(newMonth < 0){
            newMonth += 12;
            _dt.setFullYear(_dt.getFullYear() - 1); // use getFullYear instead of getYear !
        }
        _dt.setMonth(newMonth);

        this.month = (_dt.getMonth() + 1) + "/" + _dt.getFullYear();
    }

}
