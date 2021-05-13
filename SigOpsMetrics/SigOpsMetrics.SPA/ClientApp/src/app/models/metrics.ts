export class Metrics {
    dt?: Date = new Date();
    source?: string = 'main';
    level?: string = 'cor';
    interval?: string = 'mo';
    measure: string;
    start?: string = (this.dt.getMonth() + 1) + '/' + (this.dt.getFullYear() - 1);
    end?: string = (this.dt.getMonth() + 1) + '/' + this.dt.getFullYear();
    label?: string = "";
}
