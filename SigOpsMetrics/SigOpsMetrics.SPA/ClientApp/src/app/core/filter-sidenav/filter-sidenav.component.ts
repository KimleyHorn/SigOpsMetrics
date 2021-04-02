import { Component, OnInit, AfterViewInit } from "@angular/core";
import { FilterService } from "../../services/filter.service";

@Component({
  selector: "app-filter-sidenav",
  templateUrl: "./filter-sidenav.component.html",
  styleUrls: ["./filter-sidenav.component.css"],
})
export class FilterSidenavComponent implements OnInit, AfterViewInit {

  signalGroups: Array<string> = [];
  selectedSignalGroup: string;

  constructor(private filterService: FilterService) {}

  ngOnInit(): void {

  }

  ngAfterViewInit(): void {
    this.filterService.getSignalGroupsFromDb().subscribe((data) => {
      this.signalGroups = data;
      this.selectedSignalGroup = data[0]; //All RTOP
    });
  }
}
