import { Component, OnInit, AfterViewInit, Output, EventEmitter, ViewChild } from "@angular/core";
import { MatSelectionList } from "@angular/material/list";
import { FilterService } from "../../services/filter.service";

@Component({
  selector: "app-filter-sidenav",
  templateUrl: "./filter-sidenav.component.html",
  styleUrls: ["./filter-sidenav.component.css"],
})
export class FilterSidenavComponent implements OnInit, AfterViewInit {
  @ViewChild('datesSelector') datesSelector: MatSelectionList;
  @Output("toggleFilter") toggleFilter: EventEmitter<any> = new EventEmitter();
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

  resetSelections() {
    this.selectedSignalGroup = "";
    this.selectedAgency = "";  
    this.datesSelector.deselectAll();
  }
}

