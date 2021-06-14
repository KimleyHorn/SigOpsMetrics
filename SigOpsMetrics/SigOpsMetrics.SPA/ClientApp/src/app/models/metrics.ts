export class Metrics {
    dt?: Date = new Date();
    source?: string = 'main';
    level?: string = 'cor';
    interval?: string = 'mo';
    measure: string;
    start?: string = (this.dt.getMonth() + 1) + '/' + (this.dt.getFullYear() - 1);
    end?: string = (this.dt.getMonth() + 1) + '/' + this.dt.getFullYear();
    field?: string = "";
    label?: string = "";
    formatType?: string = "number";
    formatDecimals?: number = 0;
    isMapMetrics?: boolean = false;

    public constructor(fields?){
      if (this.isMapMetrics) {
        this.start = (this.dt.getMonth() + 1) + '/' + (this.dt.getFullYear());
        this.end = (this.dt.getMonth() + 1) + '/' + this.dt.getFullYear();
      }

      if(fields){
        this.dt = fields.dt || this.dt;
        this.source = fields.source || this.source;
        this.level = fields.level || this.level;
        this.interval = fields.interval || this.interval;
        this.measure = fields.measure || this.measure;
        if (fields.isMapMetrics || this.isMapMetrics) {
          this.start = (this.dt.getMonth() + 1) + '/' + (this.dt.getFullYear());
          this.end = (this.dt.getMonth() + 1) + '/' + this.dt.getFullYear();
        } else {
          this.start = fields.start || this.start;
          this.end = fields.end || this.end;
        }

        this.field = fields.field || this.field;
        this.label = fields.label || this.label;
        this.formatType = fields.formatType || this.formatType;
        this.formatDecimals = fields.formatDecimals || this.formatDecimals;
      }
    }
}
