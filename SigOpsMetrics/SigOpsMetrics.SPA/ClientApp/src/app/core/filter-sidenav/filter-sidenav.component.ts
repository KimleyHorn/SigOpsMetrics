import { Component, OnInit, AfterViewInit, Output, EventEmitter, ViewChild, ViewEncapsulation } from "@angular/core";
import { MatDatepicker } from "@angular/material/datepicker";
import { MatSelectionList } from "@angular/material/list";
import { FilterService } from "../../services/filter.service";

@Component({
  selector: "app-filter-sidenav",
  templateUrl: "./filter-sidenav.component.html",
  styleUrls: ["./filter-sidenav.component.css"],
  encapsulation: ViewEncapsulation.None
})
export class FilterSidenavComponent implements OnInit, AfterViewInit {
  @ViewChild("startDate") startDate: MatDatepicker<Date>;
  @ViewChild("endDate") endDate: MatDatepicker<Date>;
  @ViewChild("startTime") startTime: MatDatepicker<Date>;
  @ViewChild("endTime") endTime: MatDatepicker<Date>;
  @Output("toggleFilter") toggleFilter: EventEmitter<any> = new EventEmitter();
    
  //Region
  signalGroups: Array<string> = [];
  selectedSignalGroup: string;
  //District
  districts: Array<string> = [];
  selectedDistrict: string;
  //Managing Agency
  agencies: Array<string> = [];
  selectedAgency: string;
  //County
  counties: Array<string> = [];
  selectedCounty: string;
  //City
  cities: Array<string> = [];
  selectedCity: string;
  //Corridor
  corridors: Array<string> = [];
  selectedCorridor: string;
  // Data Aggregation
  timeOptions: number[] = [15,30,60];
  selectedDataAggregationOption: number;
  // Date Range
  selectedDateOption: string;
  options: string[] = ['Prior Day','Prior Quarter','Prior Week','Prior Year','Prior Month','Custom']

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

    this.filterService.getDistrictsFromDb().subscribe((data) => {
      this.districts = data;
    });

    this.filterService.getCorridorsFromDb().subscribe((data) => {
      this.corridors = data;
    });

    this.filterService.getCountiesFromDb().subscribe((data) => {
      this.counties = data;
    });

    this.filterService.getCitiesFromDb().subscribe((data) => {
      this.cities = data;
    });
  }

  updateFilter(type, e){
    this.filterService.setValue(type, e.value);
  }

  resetSelections() {
    this.selectedSignalGroup = "";
    this.selectedAgency = "";  
    this.selectedDateOption = "";
    this.selectedDistrict = "";
    this.selectedAgency = "";
    this.selectedCounty = "";
    this.selectedCity = "";
    this.selectedCorridor = "";
    this.selectedDataAggregationOption = null;
    if (this.startDate) {
      this.startDate.select(null);
    }
    if (this.endDate) {
      this.endDate.select(null);
    }
    this.startTime.select(null);
    this.endTime.select(null);
  }

}

