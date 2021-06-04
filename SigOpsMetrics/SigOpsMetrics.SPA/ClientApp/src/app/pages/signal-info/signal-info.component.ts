import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { SignalsService } from '../../services/signals.service';
import { SignalInfo } from '../../models/signal-info';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { FormGroup, FormBuilder, AbstractControl} from '@angular/forms'

@Component({
  selector: 'app-signal-info',
  templateUrl: './signal-info.component.html',
  styleUrls: ['./signal-info.component.css']
})

export class SignalInfoComponent implements OnInit, AfterViewInit {
  public signalList: SignalInfo[];
  dataSource: MatTableDataSource<SignalInfo> = new MatTableDataSource<SignalInfo>();
  displayedColumns: string[] = ['signalID', 'zoneGroup', 'zone', 'corridor', 'subcorridor', 'agency', 'mainStreetName', 'sideStreetName', 'milepost', 'asOf', 'duplicate', 'include', 'modified', 'note', 'latitude', 'longitude'];
  filterSelectList = [];
  readonly formControl: AbstractControl;

  constructor(private signalsService: SignalsService, formBuilder: FormBuilder) {
    // Object to create Filter for
    this.filterSelectList = [
      {
        name: 'Signal ID',
        columnProp: 'signalID',
        options: [],
				style: 'search-input search-input-sid'
      }, {
        name: 'Zone Gr',
        columnProp: 'zoneGroup',
        options: [],
				style: 'search-input search-input-zg'
      }, {
        name: 'Zone',
        columnProp: 'zone',
        options: [],
				style: 'search-input search-input-z'
      }, {
        name: 'Corridor',
        columnProp: 'corridor',
        options: [],
				style: 'search-input search-input-cor'
      }, {
        name: 'Subcorridor',
        columnProp: 'subcorridor',
        options: [],
				style: 'search-input search-input-scor'
      }, {
        name: 'Agency',
        columnProp: 'agency',
        options: [],
				style: 'search-input search-input-a'
      }, {
        name: 'Main Street',
        columnProp: 'mainStreetName',
        options: [],
				style: 'search-input search-input-ms'
      }, {
        name: 'Side Street',
        columnProp: 'sideStreetName',
        options: [],
				style: 'search-input search-input-ss'
      }, {
        name: 'Milepost',
        columnProp: 'milepost',
        options: [],
				style: 'search-input search-input-mp'
      }, {
        name: 'As Of',
        columnProp: 'asOf',
        options: [],
				style: 'search-input search-input-ao'
      }, {
        name: 'Duplicate',
        columnProp: 'duplicate',
        options: [],
				style: 'search-input search-input-dup'
      }, {
        name: 'Include',
        columnProp: 'include',
        options: [],
				style: 'search-input search-input-inc'
      }, {
        name: 'Modified',
        columnProp: 'modified',
        options: [],
				style: 'search-input search-input-mod'
      }, {
        name: 'Note',
        columnProp: 'note',
        options: [],
				style: 'search-input search-input-n'
      }, {
        name: 'Latitude',
        columnProp: 'latitude',
        options: [],
				style: 'search-input search-input-lat'
      }, {
        name: 'Longitude',
        columnProp: 'longitude',
        options: [],
				style: 'search-input search-input-lng'
      }
    ];

    this.dataSource.filterPredicate = ((data, filter) => {
      const a = !filter.signalID || data.signalID.toLowerCase().includes(filter.signalID);
      const b = !filter.zoneGroup || data.zoneGroup.toLowerCase().includes(filter.zoneGroup);
      const c = !filter.zone || data.zone.toLowerCase().includes(filter.zone);
      const d = !filter.corridor || data.corridor.toLowerCase().includes(filter.corridor);
      const e = !filter.subcorridor || data.subcorridor.toLowerCase().includes(filter.subcorridor);
      const f = !filter.agency || data.agency.toLowerCase().includes(filter.agency);
      const g = !filter.mainStreetName || data.mainStreetName.toLowerCase().includes(filter.mainStreetName);
      const h = !filter.sideStreetName || data.sideStreetName.toLowerCase().includes(filter.sideStreetName);
      const i = !filter.milepost || data.milepost.toLowerCase().includes(filter.milepost);
      const j = !filter.asOf || data.asOf.toLowerCase().includes(filter.asOf);
      const k = !filter.duplicate || data.duplicate.toLowerCase().includes(filter.duplicate);
      const l = !filter.include || data.include.toLowerCase().includes(filter.include);
      const m = !filter.modified || data.modified.toLowerCase().includes(filter.modified);
      const n = !filter.note || data.note.toLowerCase().includes(filter.note);
      const o = !filter.latitude || data.latitude.toLowerCase().includes(filter.latitude);
      const p = !filter.longitude || data.longitude.toLowerCase().includes(filter.longitude);
      return a && b && c && d && e && f && g && h && i && j && k && l && m && n && o && p;
    }) as (SignalInfo, string) => boolean;

    this.formControl = formBuilder.group({
      signalID: '',
      zoneGroup: '',
      zone: '',
      corridor: '',
      subcorridor: '',
      agency: '',
      mainStreetName: '',
      sideStreetName: '',
      milepost: '',
      asOf: '',
      duplicate: '',
      include: '',
      modified: '',
      note: '',
      latitude: '',
      longitude: ''
    });
    this.formControl.valueChanges.subscribe(value => {
      const filter = {...value,
        signalID: value.signalID.trim().toLowerCase(),
        zoneGroup: value.zoneGroup.trim().toLowerCase(),
        zone: value.zone.trim().toLowerCase(),
        corridor: value.corridor.trim().toLowerCase(),
        subcorridor: value.subcorridor.trim().toLowerCase(),
        agency: value.agency.trim().toLowerCase(),
        mainStreetName: value.mainStreetName.trim().toLowerCase(),
        sideStreetName: value.sideStreetName.trim().toLowerCase(),
        milepost: value.milepost.trim().toLowerCase(),
        asOf: value.asOf.trim().toLowerCase(),
        duplicate: value.duplicate.trim().toLowerCase(),
        include: value.include.trim().toLowerCase(),
        modified: value.modified.trim().toLowerCase(),
        note: value.note.trim().toLowerCase(),
        latitude: value.latitude.trim().toLowerCase(),
        longitude: value.longitude.trim().toLowerCase(),
      } as string;
      this.dataSource.filter = filter;
    });
  }

  ngOnInit(): void {
  }

  @ViewChild(MatPaginator) paginator: MatPaginator;

  ngAfterViewInit() {
    //Set the paginator before the data or performance really tanks
    this.dataSource.paginator = this.paginator;
    this.signalsService.getData().subscribe(data => this.dataSource.data = data);
  }

  // Reset table filters
  resetFilters() {
    this.filterSelectList.forEach((value, key) => {
      value.modelValue = undefined;
      this.formControl.get(value.columnProp).setValue('');
    })
    this.dataSource.filter = "";
  }
}
