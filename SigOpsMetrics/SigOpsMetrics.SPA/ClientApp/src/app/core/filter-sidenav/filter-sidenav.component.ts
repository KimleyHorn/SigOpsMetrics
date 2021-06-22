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

  selectedAggregationOption: string;
  aggregationOptions: any[] = [
    { aggregate: 'Quarterly', disabled: false, checked: false },
    { aggregate: 'Daily', disabled: false, checked: false },
    { aggregate: 'Monthly', disabled: false, checked: false },
    { aggregate: '1 hour', disabled: false, checked: false },
    { aggregate: 'Weekly', disabled: false, checked: false },
    { aggregate: '15 mins', disabled: false, checked: false },
  ]

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
    //load the zone groups for the dropdown
    this.zoneGroupsSubscription = this.filterService.zoneGroups.subscribe(data =>{
      this.signalGroups = data;
    });

    //load the zones for the dropdown
    this.zonesSubscription = this.filterService.zones.subscribe(data => {
      this.districts = data;
    });

    //load the corridors for the dropdown
    this.corridorsSubscription = this.filterService.corridors.subscribe(data => {
      this.corridors = data;
    });

    //load the agencies for the dropdown
    this.agenciesSubscription = this.filterService.agencies.subscribe(data =>{
      this.agencies = data;
    });

    //clear removed items
    this.filterSubscription = this.filterService.filters.subscribe(filter => {
      Object.keys(filter).forEach(item => {
        let value = filter[item];
        if(value === null){
          switch (item) {
            case 'dateRange':
              this._resetStartDate();
              this._resetEndDate();
              this._resetDaysOfWeek();
              this.selectedDateOption = '';
              break;
            case 'customStart':
              this._resetStartDate();
              break;
            case 'customEnd':
              this._resetEndDate();
              break;
            case 'daysOfWeek':
              this._resetDaysOfWeek();
              break;
            case 'startTime':
              this.startTime.select(null);
              break;
            case 'endTime':
              this.endTime.select(null);
              break;
            case 'dataAggregation':
              this.selectedAggregationOption = '';
              break;
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

  //unsubscribe to services
  ngOnDestroy(): void{
    this.zoneGroupsSubscription.unsubscribe();
    this.zonesSubscription.unsubscribe();
    this.corridorsSubscription.unsubscribe();
    this.agenciesSubscription.unsubscribe();
    this.filterSubscription.unsubscribe();
  }

  //update the filter on the filter service
  updateFilter(type, e){
    this.filterService.setValue(type, e.value);
  }

  //modify an aggregate field
  private _updateAggregateField(aggregate, field, value){
    return this.aggregationOptions.filter(agg => agg.aggregate === aggregate)[0][field] = value;
  }

  //reset a field for all aggregate options
  private _resetAggregateField(field, value){
    this.aggregationOptions.forEach(agg => {
      agg[field] = value;
    });
  }

  //clear the selected aggregate option
  private _clearAggregateOption(value){
    if(this.selectedAggregationOption === value){
      this.selectedAggregationOption = '';
    }
  }

  //dynamically update the available aggregates when the date ranged has changed
  updateDateRange(e){
    let value = e.value;

    this._resetAggregateField('disabled', false);

    switch (value) {
      case "Prior Day":
        this._clearAggregateOption('Daily');
        this._updateAggregateField('Daily', 'disabled', true);
      case "Prior Week":
        this._clearAggregateOption('Weekly');
        this._updateAggregateField('Weekly', 'disabled', true);
      case "Prior Month":
        this._clearAggregateOption('Monthly');
        this._updateAggregateField('Monthly', 'disabled', true);
      case "Prior Quarter":
        this._clearAggregateOption('Quarterly');
        this._updateAggregateField('Quarterly', 'disabled', true);
      default:
        break;
    }

    this.updateFilter('dateRange', e);
  }

  //clear the start date input
  private _resetStartDate(){
    if (this.startDate) {
      this.startDate.select(null);
    }
  }

  //clear the end date input
  private _resetEndDate(){
    if (this.endDate) {
      this.endDate.select(null);
    }
  }

  //reset all days of the week to be not selected
  private _resetDaysOfWeek(){
    this.daysOfWeek.forEach(element => {
      element.selected = false;
    });
  }

  //reset all options in the filter side nav to their default
  resetSelections() {
    this.selectedSignalGroup = "";
    this.selectedAgency = "";
    this.selectedDateOption = "";
    this.selectedAggregationOption = "";
    this.selectedDistrict = "";
    this.selectedAgency = "";
    this.selectedCounty = "";
    this.selectedCity = "";
    this.selectedCorridor = "";
    this.selectedDataAggregationOption = null;
    this._resetAggregateField('disabled', false);
    this._resetStartDate();
    this._resetEndDate();
    this._resetDaysOfWeek();
    this.startTime.select(null);
    this.endTime.select(null);

    this.filterService.resetFilter();
    this.toggleFilter.emit();
  }

  //apply the filter so that it loads the new data
  applyFilter(){
    this.filterService.updateFilter();
    this.toggleFilter.emit();
  }

  toggleDay(day) {
    let days: string[] = [];
    this.daysOfWeek.forEach(element => {
      if (element.day == day.day) {
        element.selected = !element.selected;
      }

      if(element.selected){
        days.push(element.day);
      }
    });

    this.filterService.setValue('daysOfWeek', days);
  }
}

