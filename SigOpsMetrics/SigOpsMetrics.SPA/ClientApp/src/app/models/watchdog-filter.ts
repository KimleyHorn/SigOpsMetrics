export class WatchdogFilter {
    startDate: Date;
    endDate: Date;
    alert: string;
    phase: string;
    intersectionFilter: string;
    streak: string;
    zoneGroup: string;
  
    constructor() {
      this.startDate = new Date(new Date().setDate(new Date().getDate()-7));
      this.endDate = new Date(new Date().setDate(new Date().getDate()-1));
      this.alert = "No Camera Image";
      this.phase = "All",
      this.intersectionFilter = "";
      this.streak = "All"

      let localStorageFilter = localStorage.getItem('filter');
      if (localStorageFilter) {
        let filter = JSON.parse(localStorageFilter);
        this.zoneGroup = filter.zone_Group;
      } else {
        this.zoneGroup = "Central Metro"
      }
    }
  }
  