import {
  Component,
  OnInit,
  AfterViewInit,
  Output,
  EventEmitter,
  ViewChild,
  ViewEncapsulation,
  ChangeDetectorRef,
} from "@angular/core";
import { MatCheckbox } from "@angular/material/checkbox";
import { MatDatepicker } from "@angular/material/datepicker";
import { Subscription } from "rxjs";
import { FilterService } from "../../services/filter.service";

@Component({
  selector: "app-filter-sidenav",
  templateUrl: "./filter-sidenav.component.html",
  styleUrls: ["./filter-sidenav.component.css"],
  encapsulation: ViewEncapsulation.None,
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
  startTime: string;
  endTime: string;
  @ViewChild("allDayCheckbox") allDayCheckbox: MatCheckbox;

  @Output("toggleFilter") toggleFilter: EventEmitter<any> = new EventEmitter();

  //Region | ZONE GROUP
  signalGroups: Array<string> = [];
  selectedSignalGroup: string = "Central Metro";
  //District | ZONE
  districts: Array<string> = [];
  selectedDistrict: string = "";
  //Managing Agency | AGENCY
  agencies: Array<string> = [];
  selectedAgency: string = "";
  //County
  counties: Array<string> = [];
  selectedCounty: string = "";
  //City
  cities: Array<string> = [];
  selectedCity: string = "";
  //Corridor | CORRIDOR
  corridors: Array<string> = [];
  selectedCorridor: string = "";
  //SubCorridor | SUBCORRIDOR
  subcorridors: Array<string> = [];
  selectedSubcorridor: string = "";

  // Priority
  priorities: Array<string> = [];
  selectedPriority: string = "";

  //Classification
  classifications: Array<string> = [];
  selectedClassification: string = "";

  // Data Aggregation
  timeOptions: number[] = [15, 30, 60];
  selectedDataAggregationOption: number;
  // Date Range
  selectedDateOption: number = 4;
  // Signal Id
  selectedSignalId: string = "";

  // Ordered like this to sort in filter
  options: any[] = [
    { value: 0, label: "Prior Day" },
    { value: 3, label: "Prior Quarter" },
    { value: 1, label: "Prior Week" },
    { value: 4, label: "Prior Year" },
    { value: 2, label: "Prior Month" },
    { value: 5, label: "Custom" },
  ];

  selectedAggregationOption: number = 4;
  aggregationOptions: any[] = [
    { value: 5, aggregate: "Quarterly", disabled: false, checked: false },
    { value: 2, aggregate: "Daily", disabled: false, checked: false },
    { value: 4, aggregate: "Monthly", disabled: false, checked: false },
    { value: 1, aggregate: "1 hour", disabled: false, checked: false },
    { value: 3, aggregate: "Weekly", disabled: false, checked: false },
    { value: 0, aggregate: "15 mins", disabled: false, checked: false },
  ];

  // Days of Week
  daysOfWeek: any[] = [
    { day: "Su", selected: false },
    { day: "Mo", selected: false },
    { day: "Tu", selected: false },
    { day: "We", selected: false },
    { day: "Th", selected: false },
    { day: "Fr", selected: false },
    { day: "Sa", selected: false },
  ];

  filterErrorSubscription: Subscription;
  zoneGroupsSubscription: Subscription;
  zonesSubscription: Subscription;
  corridorsSubscription: Subscription;
  subcorridorsSubscription: Subscription;
  agenciesSubscription: Subscription;
  filterSubscription: Subscription;
  countiesSubscription: Subscription;
  citiesSubscription: Subscription;
  prioritySubscription: Subscription;
  classificationSubscription: Subscription;

  initialLoad: boolean = true;
  inErrorState: number = 1;
  constructor(
    private filterService: FilterService,
    private changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {}

  ngAfterViewInit(): void {
    //get the current error state of the filter
    this.filterErrorSubscription = this.filterService.errorState.subscribe(
      (data) => {
        this.inErrorState = data;
      }
    );
    //load the zone groups for the dropdown
    this.zoneGroupsSubscription = this.filterService.zoneGroups.subscribe(
      (data) => {
        this.signalGroups = data;
      }
    );

    //load the zones for the dropdown
    this.zonesSubscription = this.filterService.zones.subscribe((data) => {
      this.districts = data.sort();
    });

    //load the corridors for the dropdown
    this.corridorsSubscription = this.filterService.corridors.subscribe(
      (data) => {
        this.corridors = data;
      }
    );

    //load the corridors for the dropdown
    this.subcorridorsSubscription = this.filterService.subcorridors.subscribe(
      (data) => {
        this.subcorridors = data;
      }
    );

    //load the agencies for the dropdown
    this.agenciesSubscription = this.filterService.agencies.subscribe(
      (data) => {
        this.agencies = data;
      }
    );

    //load the counties for the dropdown
    this.countiesSubscription = this.filterService.counties.subscribe(
      (data) => {
        this.counties = data;
      }
    );

    this.citiesSubscription = this.filterService.cities.subscribe((data) => {
      this.cities = data;
    });

    this.prioritySubscription = this.filterService.priorities.subscribe(
      (data) => {
        this.priorities = data;
      }
    );

    this.classificationSubscription =
      this.filterService.classifications.subscribe((data) => {
        this.classifications = data;
      });

    //clear removed items
    this.filterSubscription = this.filterService.filters.subscribe((filter) => {
      if (this.initialLoad) {
        this.initialLoad = false;
        this.syncSavedFilterOnLoad(filter);
        this.updateDateRange({ value: this.selectedDateOption });
      } else {
        Object.keys(filter).forEach((item) => {
          let value = filter[item];
          if (value === null) {
            switch (item) {
              case "dateRange":
                this._resetStartDate();
                this._resetEndDate();
                this._resetDaysOfWeek();
                this.selectedDateOption = 2;
                break;
              case "customStart":
                this._resetStartDate();
                break;
              case "customEnd":
                this._resetEndDate();
                break;
              case "daysOfWeek":
                this._resetDaysOfWeek();
                break;
              case "startTime":
                this._resetStartTime();
                break;
              case "endTime":
                this._resetEndTime();
                break;
              case "dataAggregation":
                this.selectedAggregationOption = 4;
                break;
              case "zone_Group":
                this.selectedSignalGroup = "Central Metro";
                break;
              case "zone":
                this.selectedDistrict = "";
                break;
              case "agency":
                this.selectedAgency = "";
                break;
              case "county":
                this.selectedCounty = "";
                break;
              case "city":
                this.selectedCity = "";
                break;
              case "corridor":
                this.selectedCorridor = "";
                break;
              case "subcorridor":
                this.selectedSubcorridor = "";
                break;
              case "signalId":
                this.selectedSignalId = "";
                break;
              case "priority":
                this.selectedPriority = "";
                break;
              case "classification":
                this.selectedClassification = "";
                break;
              default:
                break;
            }
          }
        });
      }
    });
  }

  //unsubscribe to services
  ngOnDestroy(): void {
    this.zoneGroupsSubscription.unsubscribe();
    this.zonesSubscription.unsubscribe();
    this.corridorsSubscription.unsubscribe();
    this.subcorridorsSubscription.unsubscribe();
    this.agenciesSubscription.unsubscribe();
    this.filterSubscription.unsubscribe();
    this.subcorridorsSubscription.unsubscribe();
    this.countiesSubscription.unsubscribe();
    this.citiesSubscription.unsubscribe();
    this.prioritySubscription.unsubscribe();
    this.classificationSubscription.unsubscribe();
    this.filterErrorSubscription.unsubscribe();
  }

  //update the filter on the filter service
  updateFilter(type, e) {
    if (type == "signalId" && e.value) {
      // clear non-date filters automatically because we're going to hide them
      this.selectedSignalGroup = "";
      this.filterService.setValue("subcorridor", "");
      this.selectedAgency = "";
      this.filterService.setValue("corridor", "");
      this.selectedDistrict = "";
      this.filterService.setValue("city", "");
      this.selectedCounty = "";
      this.filterService.setValue("county", "");
      this.selectedCity = "";
      this.filterService.setValue("agency", "");
      this.selectedCorridor = "";
      this.filterService.setValue("zone", "");
      this.selectedSubcorridor = "";
      this.filterService.setValue("zone_Group", "");
      this.selectedPriority = "";
      this.filterService.setValue("priority", "");
      this.selectedClassification = "";
      this.filterService.setValue("classification", "");
    }
    if (type === "startTime" && e.value !== '00:00' || type === "endTime" && e.value !== '23:59') {
      this.removeCheck();
    }
    this.filterService.setValue(type, e.value);
  }

  //modify an aggregate field
  private _updateAggregateField(aggregate, field, value) {
    return (this.aggregationOptions.filter(
      (agg) => agg.aggregate === aggregate
    )[0][field] = value);
  }

  //reset a field for all aggregate options
  private _resetAggregateField(field, value) {
    this.aggregationOptions.forEach((agg) => {
      agg[field] = value;
    });
  }

  //clear the selected aggregate option
  private _clearAggregateOption(value) {
    //determine which aggregation options to enable
    this.aggregationOptions.forEach((agg) => {
      if (value === 5 || agg.value === value || agg.value === value + 1) {
        agg.disabled = false;
      }
    });
    if (
      this.selectedAggregationOption < value ||
      this.selectedAggregationOption > value + 1
    ) {
      this.selectedAggregationOption = value;
      this.updateFilter("timePeriod", { value: value });
    }
  }

  //dynamically update the available aggregates when the date ranged has changed
  updateDateRange(e) {
    let value = e.value;
    this._resetAggregateField("disabled", true);

    switch (value) {
      case 5:
        let start = new Date();
        let end = new Date();
        this.changeDetectorRef.detectChanges();
        this.startDate.select(start);
        this.endDate.select(end);
        this.allDayChecked({ checked: true });
        this._clearAggregateOption(value);
        this.selectedAggregationOption = 2;
        break;
      case 0:
      case 1:
      case 2:
      case 3:
      case 4:
        this._clearAggregateOption(value);
        this._resetStartDate();
        this._resetEndDate();
        this._resetDaysOfWeek();
        this._resetStartTime();
        this._resetEndTime();
        break;
      default:
        //clear if not custom
        this._resetStartDate();
        this._resetEndDate();
        this._resetStartTime();
        this._resetEndTime();
        break;
    }
    this.updateFilter("dateRange", e);
  }

  //clear the start date input
  private _resetStartDate() {
    if (this.startDate) {
      this.startDate.select(null);
    }
  }

  //clear the end date input
  private _resetEndDate() {
    if (this.endDate) {
      this.endDate.select(null);
    }
  }

  //clear the start time input
  private _resetStartTime() {
    this.startTime = '00:00';
  }

  //clear the end time input
  private _resetEndTime() {
    this.endTime = '23:59';
  }

  //reset all days of the week to be not selected
  private _resetDaysOfWeek() {
    this.daysOfWeek.forEach((element) => {
      element.selected = false;
    });
  }

  //reset all options in the filter side nav to their default
  resetSelections() {
    this.selectedSignalGroup = "Central Metro";
    this.selectedAgency = "";
    this.selectedDateOption = 4;
    this.selectedAggregationOption = 4;
    this.selectedDistrict = "";
    this.selectedAgency = "";
    this.selectedCounty = "";
    this.selectedCity = "";
    this.selectedCorridor = "";
    this.selectedSubcorridor = "";
    this.selectedPriority = "";
    this.selectedClassification = "";
    this.selectedDataAggregationOption = null;
    this.selectedSignalId = null;
    this._resetAggregateField("disabled", true);
    this._clearAggregateOption(4);
    this._resetStartDate();
    this._resetEndDate();
    this._resetDaysOfWeek();
    this._resetStartTime();
    this._resetEndTime();

    this.filterService.resetFilter();
    //this.toggleFilter.emit();
  }

  //apply the filter so that it loads the new data
  applyFilter() {
    this.inErrorState = 1;
    this.filterService.updateFilterErrorState(1);
    this.filterService.updateFilter();
    this.toggleFilter.emit();
  }

  toggleDay(day) {
    let days: string[] = [];
    this.daysOfWeek.forEach((element) => {
      if (element.day == day.day) {
        element.selected = !element.selected;
      }

      if (element.selected) {
        days.push(element.day);
      }
    });

    this.filterService.setValue("daysOfWeek", days);
  }

  saveFilter() {
    this.applyFilter();
    this.filterService.saveCurrentFilter();
  }

  resetErrorState() {
    this.inErrorState = 1;
  }
  // private checkExistingFilter() {
  //   let localStorageFilter = localStorage.getItem('filter');
  //   if (localStorageFilter) {
  //     this.syncSavedFilterOnLoad(JSON.parse(localStorageFilter));
  //   }
  // }

  syncSavedFilterOnLoad(filterData) {
    this.selectedSignalGroup = filterData.zone_Group;
    this.selectedAgency = filterData.agency;
    this.selectedDateOption = filterData.dateRange;
    this.selectedAggregationOption = filterData.timePeriod;
    this.selectedDistrict = filterData.zone;
    this.selectedCounty = filterData.county;
    this.selectedCity = filterData.city;
    this.selectedCorridor = filterData.corridor;
    this.selectedSubcorridor = filterData.subcorridor;
    this.selectedSignalId = filterData.signalId;
    this.selectedPriority = filterData.priority;
    this.selectedClassification = filterData.classification;
  }

  allDayChecked(e) {
    if (e.checked === true) {
      this._resetStartTime();
      this._resetEndTime();
      this.updateFilter("startTime", { value: this.startTime });
      this.updateFilter("endTime", { value: this.endTime });
    }
  }

  removeCheck() {
    this.allDayCheckbox.checked = false;
  }
}
