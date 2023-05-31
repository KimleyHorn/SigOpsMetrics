import { Component, Input, OnInit } from "@angular/core";
import { Colors } from "src/app/models/colors";
import { Filter } from "src/app/models/filter";
import { Graph } from "src/app/models/graph";
import { Metrics } from "src/app/models/metrics";
import { FormatService } from "src/app/services/format.service";

@Component({
  selector: "app-line-graph",
  templateUrl: "./line-graph.component.html",
  styleUrls: ["./line-graph.component.css"],
})
export class LineGraphComponent implements OnInit {
  private color = new Colors();

  @Input() title = "";
  corridors: any;

  @Input() data: any;

  @Input() line: Graph;
  @Input() metrics: Metrics;
  lineGraph: any;
  lineData: any;

  filter: Filter;
  layout: any;
  defaultColor = this.color.gray;
  primaryColor = this.color.blue;
  secondaryColor = this.color.darkGray;

  filterErrorState: number;

  annotation1: string;
  annotation2: string;
  baseLineHoverTemplate: string;
  constructor(private formatService: FormatService) {}

  ngOnInit(): void {
    if (this.data && this.data.length > 0) {
      this.setLayout();
      this.loadLineGraph();
    } else {
      this.setEmptyGridLayout();
    }
  }

  setEmptyGridLayout() {
    this.lineGraph = {
      data: [],
      layout: {},
    };
    this.line.x = "month";
    this.layout = {
      showlegend: false,
      margin: {
        l: 10,
        r: 10,
        t: 10,
        b: 25,
      },
      xaxis: {
        showgrid: false,
        showticklabels: true,
        fixedrange: true,
      },
      yaxis: {
        showgrid: false,
        zeroline: false,
        showticklabels: false,
        fixedrange: true,
      },
      hovermode: "closest",
      annotations: [
        {
          x: 0,
          y: 0,
          text: "N/A",
          showarrow: false,
          xanchor: "right",
        }
      ],
      autosize: true,
      height: 80,
    };
  }

  setLayout() {
    this.line.x = "month";
    this.lineGraph = {
      data: [],
      layout: {},
    };
    this.layout = {
      showlegend: false,
      margin: {
        l: 10,
        r: 10,
        t: 10,
        b: 25,
      },
      xaxis: {
        showgrid: false,
        showticklabels: true,
        fixedrange: true,
      },
      yaxis: {
        showgrid: false,
        zeroline: false,
        showticklabels: false,
        fixedrange: true,
      },
      hovermode: "closest",
      annotations: [
        {
          x: this.data[0]["month"],
          y: this.data[0]["average"],
          text: this.annotation1,
          showarrow: false,
          xanchor: "right",
        },
        {
          x: this.data[this.data.length - 1]["month"],
          y: this.data[this.data.length - 1]["average"],
          text: this.annotation2,
          showarrow: false,
          xanchor: "left",
        }
      ],
      autosize: true,
      height: 90,
    };
  }

  ngOnDestroy(): void {}

  private loadLineGraph() {
    this.formatAnnotations();
    this.setLayout();

    let graphData: any[] = [];

    var avg = this.getAverage(this.data);
    var trace;
    var trace2;
    if (this.metrics.goal) {
      trace2 = {
        name: "",
        x: this.data.map((value) => new Date(value[this.line.x])),
        y: this.data.map(() => avg),
        hovertemplate: this.baseLineHoverTemplate,
        mode: "lines",
        line: {
          color: this.defaultColor,
        },
        fillcolor: this.hexToRgbA(this.line.lineColor),
        type: "scatter",
        fill: "tonexty",
      };
      this.baseLineHoverTemplate = `Goal: ${this.baseLineHoverTemplate}`;
      trace = {
        name: this.data[0].average,
        x: this.data.map((value) => new Date(value[this.line.x])),
        y: this.data.map((value) => value["average"]),
        hovertemplate: this.line.hoverTemplate,
        mode: "lines",
        line: {
          color: this.line.lineColor,
        },
        fillcolor: this.hexToRgbA(this.line.lineColor),
        type: "scatter",
        fill: "tonexty",
      };
      var traceGoal = {
        name: "",
        x: this.data.map((value) => new Date(value[this.line.x])),
        y: this.data.map(() => this.metrics.goal),
        hovertemplate: this.baseLineHoverTemplate,
        mode: "lines",
        line: {
          color: this.color.darkGray,
        },
        type: "scatter",
      };
      graphData.push(traceGoal);
    } else {
      trace = {
        name: this.data[0].average,
        x: this.data.map((value) => new Date(value[this.line.x])),
        y: this.data.map((value) => value["average"]),
        hovertemplate: this.line.hoverTemplate,
        mode: "lines",
        line: {
          color: this.line.lineColor,
        },
        type: "scatter",
      };
      trace2 = {
        name: "",
        x: this.data.map((value) => new Date(value[this.line.x])),
        y: this.data.map(() => avg),
        hovertemplate: this.baseLineHoverTemplate,
        mode: "lines",
        line: {
          color: this.defaultColor,
        },
        type: "scatter",
      };
    }

    graphData.push(trace2);
    graphData.push(trace);

    this.lineGraph.data = graphData;
  }

  //Convert hex color to rgba
  private hexToRgbA(hex: string) {
    var c;
    if (/^#([A-Fa-f0-9]{3}){1,2}$/.test(hex)) {
      c = hex.substring(1).split("");
      if (c.length == 3) {
        c = [c[0], c[0], c[1], c[1], c[2], c[2]];
      }
      c = "0x" + c.join("");
      return (
        "rgba(" +
        [(c >> 16) & 255, (c >> 8) & 255, c & 255].join(",") +
        ",0.3)"
      );
    }
    throw new Error("Bad Hex");
  }

  // Get average for data array
  private getAverage(data: any[]) {
    let sum = 0;
    for (let i = 0; i < data.length; i++) {
      sum += data[i]["average"];
    }
    return sum / data.length;
  }

  private formatAnnotations() {
    if (this.metrics.formatType === "percent") {
      this.annotation1 = this.formatService.formatPercent(
        this.data[0]["average"],
        this.metrics.formatDecimals
      );
      this.annotation2 = this.formatService.formatPercent(
        this.data[this.data.length - 1]["average"],
        this.metrics.formatDecimals
      );
      this.baseLineHoverTemplate = `<b>%{y:.${this.metrics.formatDecimals}%}</b>`;
    } else {
      this.annotation1 = this.formatService.formatNumber(
        this.data[0]["average"],
        this.metrics.formatDecimals
      );
      this.annotation2 = this.formatService.formatNumber(
        this.data[this.data.length - 1]["average"],
        this.metrics.formatDecimals
      );
      this.baseLineHoverTemplate = `<b>%{y:.${this.metrics.formatDecimals}f}</b>`;
    }
  }
}
