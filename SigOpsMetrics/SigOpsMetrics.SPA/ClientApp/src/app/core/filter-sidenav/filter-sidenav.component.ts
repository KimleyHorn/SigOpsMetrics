import { Component, OnInit, AfterViewInit } from "@angular/core";
import { MatSelectChange } from "@angular/material/select";
import { FilterService } from "../../services/filter.service";

@Component({
  selector: "app-filter-sidenav",
  templateUrl: "./filter-sidenav.component.html",
  styleUrls: ["./filter-sidenav.component.css"],
})
export class FilterSidenavComponent implements OnInit, AfterViewInit {

  signalGroups: Array<string> = [];
  agencies: Array<string> = [];
  selectedSignalGroup: string;
  selectedAgency: string;

  constructor(private filterService: FilterService) {}

  ngOnInit(): void {

  }

  ngAfterViewInit(): void {
    this.filterService.getSignalGroupsFromDb().subscribe((data) => {
      this.signalGroups = data;
      //this.selectedSignalGroup = data[0]; //All RTOP
    });

    this.filterService.getAgenciesFromDb().subscribe((data) => {
      this.agencies = data;
    });
  }

  signalGroupSelected(e: MatSelectChange){
    this.filterService.setValue("zone_Group", e.value);
  }
}
