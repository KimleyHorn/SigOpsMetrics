export class WatchdogFilter {
    startDate: Date;
    endDate: Date;
    alert: string;
    phase: string;
    intersectionFilter: string;
    streak: string;
  
    constructor() {
      this.startDate = new Date(new Date().setDate(new Date().getDate()-7));
      this.endDate = new Date();
      this.alert = "No Camera Image";
      this.phase = "All",
      this.intersectionFilter = "";
      this.streak = "All"
    }
  }
  