import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { SignalsService } from '../../services/signals.service';
import { SignalInfo } from '../../models/signal-info';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';


@Component({
  selector: 'app-signal-info',
  templateUrl: './signal-info.component.html',
  styleUrls: ['./signal-info.component.css']
})

export class SignalInfoComponent implements OnInit, AfterViewInit {
  public signalList: SignalInfo[];
  dataSource: MatTableDataSource<SignalInfo> = new MatTableDataSource<SignalInfo>();
  displayedColumns: string[] = ['signalID', 'zoneGroup', 'zone', 'corridor', 'subcorridor', 'agency', 'mainStreetName', 'sideStreetName', 'milepost', 'asOf', 'duplicate', 'include', 'modified', 'note', 'latitude', 'longitude'];
  filterValues = {};
  filterSelectList = [];
  constructor(private signalsService: SignalsService) {
    // Object to create Filter for
    this.filterSelectList = [
      {
        name: 'Signal ID',
        columnProp: 'signalID',
        options: [],
				style: 'search-input-mid-wide'
      }, {
        name: 'Zone Group',
        columnProp: 'zoneGroup',
        options: [],
				style: 'search-input-wide'
      }, {
        name: 'Zone',
        columnProp: 'zone',
        options: [],
				style: 'search-input-mid'
      }, {
        name: 'Corridor',
        columnProp: 'corridor',
        options: [],
				style: 'search-input-mid-wide'
      }, {
        name: 'Subcorridor',
        columnProp: 'subcorridor',
        options: [],
				style: 'search-input-wider'
      }, {
        name: 'Agency',
        columnProp: 'agency',
        options: [],
				style: 'search-input-mid'
      }, {
        name: 'Main Street',
        columnProp: 'mainStreetName',
        options: [],
				style: 'search-input-wider'
      }, {
        name: 'Side Street',
        columnProp: 'sideStreetName',
        options: [],
				style: 'search-input-wider'
      }, {
        name: 'Mile Post',
        columnProp: 'milepost',
        options: [],
				style: 'search-input-wide'
      }, {
        name: 'As Of',
        columnProp: 'asOf',
        options: [],
				style: 'search-input-thin'
      }, {
        name: 'Duplicate',
        columnProp: 'duplicate',
        options: [],
				style: 'search-input-wide'
      }, {
        name: 'Include',
        columnProp: 'include',
        options: [],
				style: 'search-input-mid-wide'
      }, {
        name: 'Modified',
        columnProp: 'modified',
        options: [],
				style: 'search-input-mid-wide'
      }, {
        name: 'Note',
        columnProp: 'note',
        options: [],
				style: 'search-input-mid'
      }, {
        name: 'Latitude',
        columnProp: 'latitude',
        options: [],
				style: 'search-input-wide'
      }, {
        name: 'Longitude',
        columnProp: 'longitude',
        options: [],
				style: 'search-input-wide'
      }
    ]
  }


  ngOnInit(): void {
  }

  @ViewChild(MatPaginator) paginator: MatPaginator;

  ngAfterViewInit() {
    //Set the paginator before the data or performance really tanks
    this.dataSource.paginator = this.paginator;
    this.signalsService.getData().subscribe(data => this.dataSource.data = data);
  }

  public doFilter = (value: string) => {
    this.dataSource.filter = value.trim().toLocaleLowerCase();
  }

  // Called on Filter change
  filterChange(filter, event) {
    //let filterValues = {}
    this.filterValues[filter.columnProp] = event.target.value.trim().toLowerCase()
    this.dataSource.filter = JSON.stringify(this.filterValues)
  }

  // Reset table filters
  resetFilters() {
    this.filterValues = {}
    this.filterSelectList.forEach((value, key) => {
      value.modelValue = undefined;
    })
    this.dataSource.filter = "";
  }
}
