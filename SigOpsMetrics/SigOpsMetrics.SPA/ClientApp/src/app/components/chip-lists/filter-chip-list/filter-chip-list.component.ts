import { Component, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { FilterService } from 'src/app/services/filter.service';

@Component({
  selector: 'app-filter-chip-list',
  templateUrl: './filter-chip-list.component.html',
  styleUrls: ['./filter-chip-list.component.css']
})
export class FilterChipListComponent implements OnInit {
  private _filterSubscription: Subscription;
  public filters = [];

  constructor(private _filterService: FilterService) { }

  ngOnInit(): void {
    this._filterSubscription = this._filterService.filters.subscribe(filter => {
      let mappedFilter = Object.keys(filter).map(key => {
        let value = filter[key];
        if(value !== undefined && value !== null && value !== ""){
          let name;
          switch (key) {
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
            case 'month':
              name = 'Month';
              break;
            case 'customStart':
              name = 'Custom Start';
              break;
            case 'customEnd':
              name = 'Custom End';
              break;
            case 'startTime':
              name = 'Start Time';
              break;
            case 'endTime':
              name = 'End Time';
              break;
            default:
              name = 'Custom';
              break;
          }

          return { name: name, key: key, value: filter[key] };
        }
      });


      this.filters = mappedFilter;
    });
  }

  ngOnDestroy(): void {
    this._filterSubscription.unsubscribe();
  }

  remove(filter){
    const index = this.filters.indexOf(filter);

    if(index >= 0){
      this.filters.splice(index, 1);
      console.log(this.filters);
      this._filterService.setValue(filter.key, null);
      this._filterService.updateFilter();
    }
  }
}
