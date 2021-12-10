import { Component, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { FilterService } from 'src/app/services/filter.service';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-filter-chip-list',
  templateUrl: './filter-chip-list.component.html',
  styleUrls: ['./filter-chip-list.component.css']
})
export class FilterChipListComponent implements OnInit {
  private _filterSubscription: Subscription;
  public filters = [];

  constructor(private _filterService: FilterService, private _datePipe: DatePipe) { }

  ngOnInit(): void {
    this._filterSubscription = this._filterService.filters.subscribe(filter => {
      let mappedFilter = Object.keys(filter).map(key => {
        let value = filter[key];
        if(value !== undefined && value !== null && value !== ""){
          let name;
          //set the labels for the chips and format any necessary data
          switch (key) {
            case 'month':
              name = 'Month';
              break;
            case 'dateRange':
              name = "Date Range";

              switch (value) {
                case 0:
                  value = "Prior Day";
                  break;
                case 1:
                  value = "Prior Week";
                  break;
                case 2:
                  value = "Prior Month";
                  break;
                case 3:
                  value = "Prior Quarter";
                  break;
                case 4:
                  value = "Prior Year";
                  break;
                default:
                  value = "Custom"
                  break;
              }

              break;
            case 'customStart':
              name = 'Start Date';
              value = this._datePipe.transform(value, 'MM/dd/yyyy');
              break;
            case 'customEnd':
              name = 'End Date';
              value = this._datePipe.transform(value, 'MM/dd/yyyy');
              break;
            case 'daysOfWeek':
              name = 'Day(s) of Week';
              break;
            case 'startTime':
              name = 'Start Time';
              value = this._datePipe.transform(value, 'hh:mm a');
              break;
            case 'endTime':
              name = 'End Time';
              value = this._datePipe.transform(value, 'hh:mm a');
              break;
            case 'timePeriod':
              name = 'Data Aggregation';

              switch (value) {
                case 1:
                  value = "1 hour";
                  break;
                case 2:
                  value = "Daily";
                  break;
                case 3:
                  value = "Weekly";
                  break;
                case 4:
                  value = "Monthly";
                  break;
                case 5:
                  value = "Quarterly";
                  break;
                default:
                  value = "15 mins"
                  break;
              }

              break;
            case 'zone_Group':
              name = 'Region';
              break;
            case 'zone':
              name = 'District';
              break;
            case 'agency':
              name = 'Managing Agency';
              break;
            case 'county':
              name = 'County';
              break;
            case 'city':
              name = 'City';
              break;
            case 'corridor':
              name = 'Corridor';
              break;
            default:
              name = 'Custom';
              break;
          }

          return { name: name, key: key, value: value };
        }
      });

      this.filters = mappedFilter;
    });
  }

  //unsubscribe from services
  ngOnDestroy(): void {
    this._filterSubscription.unsubscribe();
  }

  //remove a chip and re-adjust the filter
  remove(filter){
    const index = this.filters.indexOf(filter);

    if(index >= 0){
      this.filters.splice(index, 1);
      this._filterService.setValue(filter.key, null);

      if(filter.key === 'dataRange'){
        this._filterService.setValue('customStart', null);
        this._filterService.setValue('customEnd', null);
        this._filterService.setValue('daysOfWeek', null);
      }

      this._filterService.updateFilter();
    }
  }
}
