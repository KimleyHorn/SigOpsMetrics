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
  constructor(private signalsService: SignalsService) { }

  ngOnInit(): void {
    //this.getSignals();
  }

  @ViewChild(MatPaginator) paginator: MatPaginator;

  ngAfterViewInit() {
    //Set the paginator before the data or performance really tanks
    this.dataSource.paginator = this.paginator;
    this.signalsService.getData().subscribe(data => this.dataSource.data = data);
  }

  getSignals(): void {
    //this.signalList = this.signalsService.signalInfos;
  }
}
