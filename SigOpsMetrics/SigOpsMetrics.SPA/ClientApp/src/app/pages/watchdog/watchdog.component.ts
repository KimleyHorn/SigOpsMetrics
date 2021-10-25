import {
  Component,
  EventEmitter,
  OnInit,
  Output,
  ViewChild,
} from "@angular/core";
import { MatPaginator } from "@angular/material/paginator";
import { MatSort } from "@angular/material/sort";
import { MatTable, MatTableDataSource } from "@angular/material/table";
import { Title } from "@angular/platform-browser";
import { PlotlyComponent } from "angular-plotly.js";
import { BehaviorSubject, Subject } from "rxjs";
import { debounceTime, distinctUntilChanged } from "rxjs/operators";
import { WatchdogFilter } from "src/app/models/watchdog-filter";
import { FilterService } from "src/app/services/filter.service";
import { WatchdogService } from "src/app/services/watchdog-service";
declare const Plotly;

@Component({
  selector: "app-watchdog",
  templateUrl: "./watchdog.component.html",
  styleUrls: ["./watchdog.component.css"],
})
export class WatchdogComponent implements OnInit {
  @ViewChild(MatSort) sort: MatSort;
  @ViewChild(MatPaginator) paginator: MatPaginator;
  isLoading: boolean = false;
  layout: Object;
  plotData: any[] = [];
  tableData;
  displayedColumns: string[] = [
    "zone",
    "corridor",
    "signalID",
    "name",
    "alert",
    "occurrences",
    "streak",
  ];
  alerts: string[] = [
    "No Camera Image",
    "Bad Vehicle Detection",
    "Bad Ped Pushbuttons",
    "Pedestrian Activations",
    "Force Offs",
    "Max Outs",
    "Count",
    "Missing Records",
  ];
  phases: string[] = ["All", "1", "2", "3", "4", "5", "6", "7", "8"];
  zoneGroups: string[] = ["Central Metro","East Metro","West Metro","North","Southeast","Southwest","Ramp Meters"];
  streaks: string[] = ["All", "Active", "Active 3-days"];
  filter: WatchdogFilter = new WatchdogFilter();
  filterSubject: Subject<WatchdogFilter> = new Subject<WatchdogFilter>();

  constructor(
    private titleService: Title,
    private filterService: WatchdogService,
    private siteFilterService: FilterService
  ) {
    this.tableData = new MatTableDataSource([]);

    this.plotData.push({
      x: [],
      y: [],
      z: [],
      type: "heatmap",
      hoverongaps: false,
      xgap: 1,
      ygap: 1,
      colorscale: [
        ["0", "rgb(237,237,237)"],
        [".01111111111", "rgb(245,158,103)"],
        ["1", "rgb(153,62,5)"],
      ],
      colorbar: {
        len: 0.25,
        title: "streak",
      },
      //text: text,
      hovertemplate:
        "<b>Name:</b> %{y}<br><b>Date:</b> %{x}<br><b>Streak:</b> %{z}" +
        "<extra></extra>",
    });
    this.filterSubject.pipe(debounceTime(1000)).subscribe((filterUpdate) => {
      if (filterUpdate.startDate && filterUpdate.endDate) {
        this.isLoading = true;
        this.filterService.loadFilteredData(this.filter);
      }
    });
  }

  ngOnInit(): void {
    this.titleService.setTitle("SigOpsMetrics - Watchdog");
    this.filterService.data.subscribe((data) => {
      this.getLayout(data);
      if (data) {
        this.tableData.data = data[0].tableData;
        this.plotData[0].x = data[0].x;
        this.plotData[0].y = data[0].y;
        this.plotData[0].z = data[0].z;
        this.plotData[0].colorbar.len = this.getLegendLen(data[0].y.length);

        try {
          Plotly.newPlot("plot", this.plotData, this.layout, {
            responsive: true,
          });
        } catch (error) {
          console.error(error);
        }
        this.isLoading = false;
      }
    });
    this.filterChange();
    this.siteFilterService.updateFilterErrorState(3);
  }
  ngAfterViewInit() {
    this.tableData.sort = this.sort;
    this.tableData.paginator = this.paginator;
  }

  filterChange() {
    this.filterSubject.next(this.filter);
  }

  getLayout(data): void {
    if (data && data[0].z.length > 0) {
      this.layout = {
        font: {
          size: 10,
        },
        height: data[0].y.length * 18 + 200,
        // width: this.getGridWidth(data[0].x.length, data[0].y),
        yaxis: {
          automargin: true,
          tickmode: "auto",
          nticks: data[0].y.length,
        },
        xaxis: {
          side: "top",
          tickformat: "%B %d",
          tickmode: "linear",
        },
      };
    } else {
      this.layout = {
        height: 30,
        xaxis: {
          visible: false,
        },
        yaxis: {
          visible: false,
        },
        annotations: [
          {
            text: "No data",
            xref: "paper",
            yref: "paper",
            showarrow: false,
            font: {
              size: 28,
            },
          },
        ],
      };
    }
  }

  getLegendLen(length): number {
    if (length >= 1 && length <= 3) {
      return 2;
    } else if (length > 3 && length <= 7) {
      return 1;
    } else if (length > 7 && length <= 20) {
      return 0.5;
    } else {
      return 0.25;
    }
  }
  // Logic to handle dynamic cell width sizing. maybe won't need.
  // getGridWidth(length, yAxisLabels):number {
  //   var maxLabelLength = 0;
  //   yAxisLabels.forEach(element => {
  //     if (element.length > maxLabelLength) {
  //       maxLabelLength = element.length;
  //     }
  //   })

  //   var width = maxLabelLength * 5;
  //   width += length * 125;
  //   console.log(width);
  //   return width > 1665 ? 1665 : width;
  // }
}
