import { Component, OnInit, AfterViewInit, Output, EventEmitter, ViewChild, ViewEncapsulation } from "@angular/core";
import { MatDatepicker } from "@angular/material/datepicker";
import { MatSelectionList } from "@angular/material/list";
import { Subscription } from "rxjs";
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

  //Region | ZONE GROUP
  signalGroups: Array<string> = [];
  selectedSignalGroup: string;
  //District | ZONE
  districts: Array<string> = [];
  selectedDistrict: string;
  //Managing Agency | AGENCY
  agencies: Array<string> = [];
  selectedAgency: string;
  //County
  counties: Array<string> = [];
  selectedCounty: string;
  //City
  cities: Array<string> = [];
  selectedCity: string;
  //Corridor | CORRIDOR
  corridors: Array<string> = [];
  selectedCorridor: string;
  // Data Aggregation
  timeOptions: number[] = [15,30,60];
  selectedDataAggregationOption: number;
  // Date Range
  selectedDateOption: string;
  options: string[] = ['Prior Day','Prior Quarter','Prior Week','Prior Year','Prior Month','Custom']
  // Days of Week
  daysOfWeek: any[] = [
    {day:'Su',selected: false},
    {day:'Mo',selected: false},
    {day:'Tu',selected: false},
    {day:'We',selected: false},
    {day:'Th',selected: false},
    {day:'Fr',selected: false},
    {day:'Sa',selected: false},
  ];

  zoneGroupsSubscription: Subscription;
  zonesSubscription: Subscription;
  corridorsSubscription: Subscription;
  agenciesSubscription: Subscription;
  filterSubscription: Subscription;

  constructor(private filterService: FilterService) {}

  ngOnInit(): void {

  }

  ngAfterViewInit(): void {
    this.zoneGroupsSubscription = this.filterService.zoneGroups.subscribe(data =>{
      this.signalGroups = data;
    });

    this.zonesSubscription = this.filterService.zones.subscribe(data => {
      this.districts = data;
    });

    this.corridorsSubscription = this.filterService.corridors.subscribe(data => {
      this.corridors = data;
    });

    this.agenciesSubscription = this.filterService.agencies.subscribe(data =>{
      this.agencies = data;
    });

    this.filterSubscription = this.filterService.filters.subscribe(filter => {
      console.log(filter);
      Object.keys(filter).forEach(item => {
        let value = filter[item];
        if(value === null){
          switch (item) {
            case 'zone_Group':
              this.selectedSignalGroup = '';
              break;
            case 'zone':
              this.selectedDistrict = '';
              break;
            case 'agency':
              this.selectedAgency = '';
              break;
            case 'county':
              this.selectedCounty = '';
              break;
            case 'city':
              this.selectedCity = '';
              break;
            case 'corridor':
              this.selectedCorridor = '';
              break;
            default:
              break;
          }
        }
      });
    });
  }

  ngOnDestroy(): void{
    this.zoneGroupsSubscription.unsubscribe();
    this.zonesSubscription.unsubscribe();
    this.corridorsSubscription.unsubscribe();
    this.agenciesSubscription.unsubscribe();
    this.filterSubscription.unsubscribe();
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
    this.daysOfWeek.forEach(element => {
      element.selected = false;
    });

    this.filterService.resetFilter();
    this.toggleFilter.emit();
  }

  applyFilter(){
    this.filterService.updateFilter();
    this.toggleFilter.emit();
  }

  toggleDay(day) {
    this.daysOfWeek.forEach(element => {
      if (element.day == day.day) {
        element.selected = !element.selected;
      }
    })
  }
}

