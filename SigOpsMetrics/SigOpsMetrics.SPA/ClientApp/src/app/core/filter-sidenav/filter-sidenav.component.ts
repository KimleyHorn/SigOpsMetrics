import { Component, OnInit, AfterViewInit, Output, EventEmitter, ViewChild, ViewEncapsulation, ChangeDetectorRef } from "@angular/core";
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
  // private startDate: MatDatepicker<Date>;
  // @ViewChild('startDate', {static: false}) set content(content: MatDatepicker<Date>){
  //   if(content){
  //     this.startDate = content;
  //   }
  // }
  @ViewChild("startDate", { static: false }) startDate: MatDatepicker<Date>;
  @ViewChild("endDate", { static: false }) endDate: MatDatepicker<Date>;
  @ViewChild("startTime", { static: false }) startTime: MatDatepicker<Date>;
  @ViewChild("endTime", { static: false }) endTime: MatDatepicker<Date>;

  @Output("toggleFilter") toggleFilter: EventEmitter<any> = new EventEmitter();

  //Region | ZONE GROUP
  signalGroups: Array<string> = [];
  selectedSignalGroup: string = 'All';
  //District | ZONE
  districts: Array<string> = [];
  selectedDistrict: string = '';
  //Managing Agency | AGENCY
  agencies: Array<string> = [];
  selectedAgency: string = '';
  //County
  counties: Array<string> = [];
  selectedCounty: string = '';
  //City
  cities: Array<string> = [];
  selectedCity: string = '';
  //Corridor | CORRIDOR
  corridors: Array<string> = [];
  selectedCorridor: string = '';
  //SubCorridor | SUBCORRIDOR
  subcorridors: Array<string> = [];
  selectedSubcorridor: string = '';
  // Data Aggregation
  timeOptions: number[] = [15,30,60];
  selectedDataAggregationOption: number;
  // Date Range
  selectedDateOption: number = 4;
  // Signal Id
  selectedSignalId: string = "";
  options: any[] = [
    { value: 0, label: 'Prior Day'},
    { value: 3, label: 'Prior Quarter'},
    { value: 1, label: 'Prior Week'},
    { value: 4, label: 'Prior Year'},
    { value: 2, label: 'Prior Month'},
    { value: 5, label: 'Custom' }
  ]

  selectedAggregationOption: number = 4;
  aggregationOptions: any[] = [
    { value: 5, aggregate: 'Quarterly', disabled: false, checked: false },
    { value: 2, aggregate: 'Daily', disabled: false, checked: false },
    { value: 4, aggregate: 'Monthly', disabled: false, checked: false },
    { value: 1, aggregate: '1 hour', disabled: false, checked: false },
    { value: 3, aggregate: 'Weekly', disabled: false, checked: false },
    { value: 0, aggregate: '15 mins', disabled: false, checked: false },
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
  subcorridorsSubscription: Subscription;
  agenciesSubscription: Subscription;
  filterSubscription: Subscription;

  constructor(private filterService: FilterService, private changeDetectorRef: ChangeDetectorRef) {}

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

      //load the corridors for the dropdown
      this.subcorridorsSubscription = this.filterService.subcorridors.subscribe(data => {
        this.subcorridors = data;
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
              this.selectedDateOption = 2;
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
              this._resetStartTime();
              break;
            case 'endTime':
              this._resetEndTime();
              break;
            case 'dataAggregation':
              this.selectedAggregationOption = 4;
              break;
            case 'zone_Group':
              this.selectedSignalGroup = 'All';
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
            case 'subcorridor':
              this.selectedSubcorridor = '';
              break;
            case 'signalId':
              this.selectedSignalId = '';
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
    this.subcorridorsSubscription.unsubscribe();
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
    if(this.selectedAggregationOption > value + 1){
      this.selectedAggregationOption = null;
    }
  }

  //dynamically update the available aggregates when the date ranged has changed
  updateDateRange(e){
    let value = e.value;

    this._resetAggregateField('disabled', false);

    switch (value) {
      case 5:
        let dt = new Date();
        this.changeDetectorRef.detectChanges();
        this.startDate.select(dt);
        this.endDate.select(dt);
        this.startTime.select(dt);
        this.endTime.select(dt);
        break;
      case 0:
        this._clearAggregateOption(value);
        this._updateAggregateField('Daily', 'disabled', true);
      case 1:
        this._clearAggregateOption(value);
        this._updateAggregateField('Weekly', 'disabled', true);
      case 2:
        this._clearAggregateOption(value);
        this._updateAggregateField('Monthly', 'disabled', true);
      case 3:
        this._clearAggregateOption(value);
        this._updateAggregateField('Quarterly', 'disabled', true);
      default:
        //clear if not custom
        this._resetStartDate();
        this._resetEndDate();
        this._resetStartTime();
        this._resetEndTime();
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

  //clear the start time input
  private _resetStartTime(){
    if (this.startTime) {
      this.startTime.select(null);
    }
  }

  //clear the end time input
  private _resetEndTime(){
    if (this.endTime) {
      this.endTime.select(null);
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
    this.selectedDateOption = 2;
    this.selectedAggregationOption = 4;
    this.selectedDistrict = "";
    this.selectedAgency = "";
    this.selectedCounty = "";
    this.selectedCity = "";
    this.selectedCorridor = "";
    this.selectedSubcorridor = "";
    this.selectedDataAggregationOption = null;
    this.selectedSignalId = null;
    this._resetAggregateField('disabled', false);
    this._resetStartDate();
    this._resetEndDate();
    this._resetDaysOfWeek();
    this._resetStartTime();
    this._resetEndTime();

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

