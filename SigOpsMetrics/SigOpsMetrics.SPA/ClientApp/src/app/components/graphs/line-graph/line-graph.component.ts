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
    var trace: { name;x;y;hovertemplate: string;mode: string;line: { color: string };type: string;fill: string } |
               { name;x;y;hovertemplate: string;mode: string;line: { color: string };type: string };
    var trace2: { name: string;x;y;hovertemplate: string;mode: string;line: { color: string };type: string };
    if (this.metrics.goal) {
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
        type: "scatter",
        fill: "tonexty",
      };
      trace2 = {
        name: "",
        x: this.data.map((value) => new Date(value[this.line.x])),
        y: this.data.map(() => this.metrics.goal),
        hovertemplate: this.baseLineHoverTemplate,
        mode: "lines",
        line: {
          color: this.defaultColor,
        },
        type: "scatter",
      };
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
        y: this.data.map(() => this.data[0]["average"]),
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
