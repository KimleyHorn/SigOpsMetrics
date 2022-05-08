import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';

import { ChartToggleService } from 'src/app/components/toggles/chart-toggle/chart-toggle.service';
import { Graph } from 'src/app/models/graph';
import { MapSettings } from 'src/app/models/map-settings';
import { Metrics } from 'src/app/models/metrics';
import { MetricsService } from '../../services/metrics.service';
import { HealthMaintenance } from '../../models/health-maintenance';
import { HealthOperations } from '../../models/health-operations';
import { HealthSafety } from '../../models/health-safety';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import {Sort} from '@angular/material/sort';
import { FormGroup, FormBuilder, AbstractControl} from '@angular/forms'
import { MatTabChangeEvent } from '@angular/material/tabs';
import { Title } from '@angular/platform-browser';
import { FilterService } from 'src/app/services/filter.service';

@Component({
  selector: 'app-health-metrics',
  templateUrl: './health-metrics.component.html',
  styleUrls: ['./health-metrics.component.css'],
  providers: [MapSettings]
})

export class HealthMetricsComponent implements OnInit {
  northData: number[] = [];
  southWestData: number[] = [];
  southEastData: number[] = [];
  westernMetroData: number[] = [];
  centralMetroData: number[] = [];
  easternMetroData: number[] = [];
  statewideData: number[] = [];

  toggleValue: string;
  currentTab: string;

  mtGraphMetrics: Metrics = new Metrics({ measure: "maint_plot", formatDecimals: 1, formatType: "percent"  });
  mtTitle: string = 'Percent Health';
  mtBar: Graph = {
    title: 'Selected Month',
    x: 'percent Health',
    y: 'corridor',
    hoverTemplate:
      '<b>%{y}</b>' +
      '<br>Percent Health: <b>%{x:.1%}</b>' +
      '<extra></extra>',
  };
  mtLine: Graph = {
    title: 'Weekly Trend',
    x: 'month',
    y: 'percent Health',
    text: 'corridor',
    hoverTemplate:
      '<b>%{text}</b>' +
      '<br>Week of: <b>%{x}</b>' +
      '<br>Percent Health: <b>%{y:.1%}</b>' +
      '<extra></extra>'
  };


  otGraphMetrics: Metrics = new Metrics({ measure: "ops_plot", formatDecimals: 1, formatType: "percent"  });
  otTitle: string = 'Percent Health';
  otBar: Graph = {
    title: 'Selected Month',
    x: 'percent Health',
    y: 'corridor',
    hoverTemplate:
      '<b>%{y}</b>' +
      '<br>Percent Health: <b>%{x:.1%}</b>' +
      '<extra></extra>',
  };
  otLine: Graph = {
    title: 'Weekly Trend',
    x: 'month',
    y: 'percent Health',
    text: 'corridor',
    hoverTemplate:
      '<b>%{text}</b>' +
      '<br>Week of: <b>%{x}</b>' +
      '<br>Percent Health: <b>%{y:.1%}</b>' +
      '<extra></extra>'
  };

  stGraphMetrics: Metrics = new Metrics({ measure: "safety_plot", formatDecimals: 1, formatType: "percent" });
  stTitle: string = 'Percent Health';
  stBar: Graph = {
    title: 'Selected Month',
    x: 'percent Health',
    y: 'corridor',
    hoverTemplate:
      '<b>%{y}</b>' +
      '<br>Percent Health: <b>%{x:.1%}</b>' +
      '<extra></extra>',
  };
  stLine: Graph = {
    title: 'Weekly Trend',
    x: 'month',
    y: 'percent Health',
    text: 'corridor',
    hoverTemplate:
      '<b>%{text}</b>' +
      '<br>Week of: <b>%{x}</b>' +
      '<br>Percent Health: <b>%{y:.1%}</b>' +
      '<extra></extra>'
  };

  dataSourceMaintenance: MatTableDataSource<HealthMaintenance> = new MatTableDataSource<HealthMaintenance>();
  displayedColumnsMaintenance: string[] = ['zone_Group', 'corridor', 'percent Health', 'missingData', 'detection Uptime Score', 'ped Actuation Uptime Score', 'comm Uptime Score', 'cctv Uptime Score', 'flash Events Score', 'detection Uptime', 'ped Actuation Uptime', 'comm Uptime', 'cctv Uptime', 'flash Events'];
  filterSelectListMaintenance = [];
  sortedDataMaintenance: HealthMaintenance[];
  readonly formControlMaintenance: AbstractControl;

  dataSourceOperations: MatTableDataSource<HealthOperations> = new MatTableDataSource<HealthOperations>();
  displayedColumnsOperations: string[] = ['zone_Group', 'corridor', 'percent Health', 'missingData', 'platoon Ratio Score', 'ped Delay Score', 'split Failures Score', 'travel Time Index Score', 'buffer Index Score', 'platoon Ratio', 'ped Delay', 'split Failures', 'travel Time Index', 'buffer Index'];
  filterSelectListOperations = [];
  sortedDataOperations: HealthOperations[];
  readonly formControlOperations: AbstractControl;

  dataSourceSafety: MatTableDataSource<HealthSafety> = new MatTableDataSource<HealthSafety>();
  displayedColumnsSafety: string[] = ['zone_Group', 'corridor', 'percent Health', 'missingData', 'crash Rate Index Score', 'kabco Crash Severity Index Score', 'high Speed Index Score', 'ped Injury Exposure Index Score', 'crash Rate Index', 'kabco Crash Severity Index', 'high Speed Index', 'ped Injury Exposure Index'];
  filterSelectListSafety = [];
  sortedDataSafety: HealthSafety[];
  readonly formControlSafety: AbstractControl;

  constructor(private metricsService: MetricsService, formBuilder: FormBuilder, private toggleService: ChartToggleService, public mapSettings: MapSettings, private titleService:Title, private filterService:FilterService) {
    // Object to create Filter for
    this.filterSelectListMaintenance = [
      {
        name: 'Zone Gr',
        columnProp: 'zone_Group',
        options: [],
				style: 'search-input search-input-zg'
      }, {
        name: 'Corridor',
        columnProp: 'corridor',
        options: [],
				style: 'search-input search-input-cor'
      }, {
        name: 'Percent Health',
        columnProp: 'percentHealth',
        options: [],
				style: 'search-input search-input-ph'
      }, {
        name: 'Missing Data',
        columnProp: 'missingData',
        options: [],
				style: 'search-input search-input-md'
      }, {
        name: 'Detection Uptime Score',
        columnProp: 'detectionUptimeScore',
        options: [],
				style: 'search-input search-input-dus search-input-score-m'
      }, {
        name: 'Ped Actuation Uptime Score',
        columnProp: 'pedActuationUptimeScore',
        options: [],
				style: 'search-input search-input-paus search-input-score-m'
      }, {
        name: 'Comm Uptime Score',
        columnProp: 'commUptimeScore',
        options: [],
				style: 'search-input search-input-cus search-input-score-m'
      }, {
        name: 'CCTV Uptime Score',
        columnProp: 'cctvUptimeScore',
        options: [],
				style: 'search-input search-input-ccus search-input-score-m'
      }, {
        name: 'Flash Events Score',
        columnProp: 'flashEventsScore',
        options: [],
				style: 'search-input search-input-fes search-input-score-m'
      }, {
        name: 'Detection Uptime',
        columnProp: 'detectionUptime',
        options: [],
				style: 'search-input search-input-du search-input-metric-m'
      }, {
        name: 'Ped Actuation Uptime',
        columnProp: 'pedDelay',
        options: [],
				style: 'search-input search-input-pau search-input-metric-m'
      }, {
        name: 'Comm Uptime',
        columnProp: 'commUptime',
        options: [],
				style: 'search-input search-input-cu search-input-metric-m'
      }, {
        name: 'CCTV Uptime',
        columnProp: 'cctvUptime',
        options: [],
				style: 'search-input search-input-ccu search-input-metric-m'
      }, {
        name: 'Flash Events',
        columnProp: 'flashEvents',
        options: [],
				style: 'search-input search-input-fe search-input-metric-m'
      }
    ];

    this.dataSourceMaintenance.filterPredicate = ((data, filter) => {
      const a = !filter.zone_Group || data.zone_Group.toLowerCase().includes(filter.zone_Group);
      const b = !filter.corridor || data.corridor.toLowerCase().includes(filter.corridor);
      const c = !filter.percentHealth || this.evaluateFilter(data, 'percent Health', filter.percentHealth, true);
      const d = !filter.missingData || this.evaluateFilter(data, 'missing Data', filter.missingData, true);
      const e = !filter.detectionUptimeScore || this.evaluateFilter(data, 'detection Uptime Score', filter.detectionUptimeScore, false);
      const f = !filter.pedActuationUptimeScore || this.evaluateFilter(data, 'ped Actuation Uptime Score',filter.pedActuationUptimeScore, false);
      const g = !filter.commUptimeScore || this.evaluateFilter(data, 'comm Uptime Score', filter.commUptimeScore, false);
      const h = !filter.cctvUptimeScore || this.evaluateFilter(data, 'cctv Uptime Score', filter.cctvUptimeScore, false);
      const i = !filter.flashEventsScore || this.evaluateFilter(data, 'flash Events Score', filter.flashEventsScore, false);
      const j = !filter.detectionUptime || this.evaluateFilter(data, 'detection Uptime Score', filter.detectionUptime, true);
      const k = !filter.pedActuationUptime || this.evaluateFilter(data, 'ped Actuation uptime', filter.pedActuationUptime, true);
      const l = !filter.commUptime || this.evaluateFilter(data, 'comm Uptime', filter.commUptime, true);
      const m = !filter.cctvUptime || this.evaluateFilter(data, 'cctv Uptime', filter.cctvUptime, true);
      const n = !filter.flashEvents || this.evaluateFilter(data, 'flash Events', filter.flashEvents, false);
      return a && b && c && d && e && f && g && h && i && j && k && l && m && n;
    }) as (HealthMaintenance, string) => boolean;

    this.formControlMaintenance = formBuilder.group({
      zone_Group: '',
      corridor: '',
      percentHealth: '',
      missingData: '',
      detectionUptimeScore: '',
      pedActuationUptimeScore: '',
      commUptimeScore: '',
      cctvUptimeScore: '',
      flashEventsScore: '',
      detectionUptime: '',
      pedDelay: '',
      commUptime: '',
      cctvUptime: '',
      flashEvents: ''
    });
    this.formControlMaintenance.valueChanges.subscribe(value => {
      const filter = {...value,
        zone_Group: value.zone_Group.trim().toLowerCase(),
        corridor: value.corridor.trim().toLowerCase(),
        percentHealth: value.percentHealth.trim().toLowerCase(),
        missingData: value.missingData.trim().toLowerCase(),
        detectionUptimeScore: value.detectionUptimeScore.trim().toLowerCase(),
        pedActuationUptimeScore: value.pedActuationUptimeScore.trim().toLowerCase(),
        commUptimeScore: value.commUptimeScore.trim().toLowerCase(),
        cctvUptimeScore: value.cctvUptimeScore.trim().toLowerCase(),
        flashEventsScore: value.flashEventsScore.trim().toLowerCase(),
        detectionUptime: value.detectionUptime.trim().toLowerCase(),
        pedDelay: value.pedDelay.trim().toLowerCase(),
        commUptime: value.commUptime.trim().toLowerCase(),
        cctvUptime: value.cctvUptime.trim().toLowerCase(),
        flashEvents: value.flashEvents.trim().toLowerCase(),
      } as string;
      this.dataSourceMaintenance.filter = filter;
    });

    this.filterSelectListOperations = [
      {
        name: 'Zone Gr',
        columnProp: 'zone_Group',
        options: [],
				style: 'search-input search-input-zg'
      }, {
        name: 'Corridor',
        columnProp: 'corridor',
        options: [],
				style: 'search-input search-input-cor'
      }, {
        name: 'Percent Health',
        columnProp: 'percentHealth',
        options: [],
				style: 'search-input search-input-ph'
      }, {
        name: 'Missing Data',
        columnProp: 'missingData',
        options: [],
				style: 'search-input search-input-md'
      }, {
        name: 'Platoon Ratio Score',
        columnProp: 'platoonRatioScore',
        options: [],
				style: 'search-input search-input-prs search-input-score-ops'
      }, {
        name: 'Ped Delay Score',
        columnProp: 'pedDelayScore',
        options: [],
				style: 'search-input search-input-pds search-input-score-ops'
      }, {
        name: 'Split Failures Score',
        columnProp: 'splitFailuresScore',
        options: [],
				style: 'search-input search-input-sfs search-input-score-ops'
      }, {
        name: 'Travel Time Index Score',
        columnProp: 'travelTimeIndexScore',
        options: [],
				style: 'search-input search-input-ttis search-input-score-ops'
      }, {
        name: 'Buffer Index Score',
        columnProp: 'bufferIndexScore',
        options: [],
				style: 'search-input search-input-bis search-input-score-ops'
      }, {
        name: 'Platoon Ratio',
        columnProp: 'platoonRatio',
        options: [],
				style: 'search-input search-input-pr search-input-metric-ops'
      }, {
        name: 'Ped Delay',
        columnProp: 'pedDelay',
        options: [],
				style: 'search-input search-input-pd search-input-metric-ops'
      }, {
        name: 'Split Failures',
        columnProp: 'splitFailures',
        options: [],
				style: 'search-input search-input-sf search-input-metric-ops'
      }, {
        name: 'Travel Time Index',
        columnProp: 'travelTimeIndex',
        options: [],
				style: 'search-input search-input-tti search-input-metric-ops'
      }, {
        name: 'Buffer Index',
        columnProp: 'bufferIndex',
        options: [],
				style: 'search-input search-input-bi search-input-metric-ops'
      }
    ];
    this.dataSourceOperations.filterPredicate = ((data, filter) => {
      const a = !filter.zone_Group || data.zone_Group.toLowerCase().includes(filter.zone_Group);
      const b = !filter.corridor || data.corridor.toLowerCase().includes(filter.corridor);
      const c = !filter.percentHealth || this.evaluateFilter(data, "percent Health", filter.percentHealth, true);
      const d = !filter.missingData || this.evaluateFilter(data, "missing Data", filter.missingData, true);
      const e = !filter.platoonRatioScore || this.evaluateFilter(data,"platoon Ratio Score", filter.platoonRatioScore, false);
      const f = !filter.pedDelayScore || this.evaluateFilter(data, "ped Delay Score", filter.pedDelayScore, false);
      const g = !filter.splitFailuresScore || this.evaluateFilter(data, "split Failures Score", filter.splitFailuresScore, false);
      const h = !filter.travelTimeIndexScore || this.evaluateFilter(data, "travel Time Index Score", filter.travelTimeIndexScore, false);
      const i = !filter.bufferIndexScore || this.evaluateFilter(data, "buffer Index Score", filter.bufferIndexScore, false);
      const j = !filter.platoonRatio || this.evaluateFilter(data, "platoon Ratio", filter.platoonRatio, false);
      const k = !filter.pedDelay || this.evaluateFilter(data, "ped Delay", filter.pedDelay, false);
      const l = !filter.splitFailures || this.evaluateFilter(data, "split Failures", filter.splitFailures, true);
      const m = !filter.travelTimeIndex || this.evaluateFilter(data, "travel time Index", filter.travelTimeIndex, false);
      const n = !filter.bufferIndex || this.evaluateFilter(data, "buffer Index", filter.bufferIndex, false);
      return a && b && c && d && e && f && g && h && i && j && k && l && m && n;
    }) as (HealthOperations, string) => boolean;

    this.formControlOperations = formBuilder.group({
      zone_Group: '',
      corridor: '',
      percentHealth: '',
      missingData: '',
      platoonRatioScore: '',
      pedDelayScore: '',
      splitFailuresScore: '',
      travelTimeIndexScore: '',
      bufferIndexScore: '',
      platoonRatio: '',
      pedDelay: '',
      splitFailures: '',
      travelTimeIndex: '',
      bufferIndex: ''
    });
    this.formControlOperations.valueChanges.subscribe(value => {
      const filter = {...value,
        zone_Group: value.zone_Group.trim().toLowerCase(),
        corridor: value.corridor.trim().toLowerCase(),
        percentHealth: value.percentHealth.trim().toLowerCase(),
        missingData: value.missingData.trim().toLowerCase(),
        platoonRatioScore: value.platoonRatioScore.trim().toLowerCase(),
        pedDelayScore: value.pedDelayScore.trim().toLowerCase(),
        splitFailuresScore: value.splitFailuresScore.trim().toLowerCase(),
        travelTimeIndexScore: value.travelTimeIndexScore.trim().toLowerCase(),
        bufferIndexScore: value.bufferIndexScore.trim().toLowerCase(),
        platoonRatio: value.platoonRatio.trim().toLowerCase(),
        pedDelay: value.pedDelay.trim().toLowerCase(),
        splitFailures: value.splitFailures.trim().toLowerCase(),
        travelTimeIndex: value.travelTimeIndex.trim().toLowerCase(),
        bufferIndex: value.bufferIndex.trim().toLowerCase(),
      } as string;
      this.dataSourceOperations.filter = filter;
    });

    this.filterSelectListSafety = [
      {
        name: 'Zone Gr',
        columnProp: 'zone_Group',
        options: [],
				style: 'search-input search-input-zg'
      }, {
        name: 'Corridor',
        columnProp: 'corridor',
        options: [],
				style: 'search-input search-input-cor'
      }, {
        name: 'Percent Health',
        columnProp: 'percentHealth',
        options: [],
				style: 'search-input search-input-ph'
      }, {
        name: 'Missing Data',
        columnProp: 'missingData',
        options: [],
				style: 'search-input search-input-md'
      }, {
        name: 'Crash Rate Index Score',
        columnProp: 'crashRateIndexScore',
        options: [],
				style: 'search-input search-input-cris search-input-score-s'
      }, {
        name: 'Kabco Crash Severity Index Score',
        columnProp: 'kabcoCrashSeverityIndexScore',
        options: [],
				style: 'search-input search-input-kcsis search-input-score-s'
      }, {
        name: 'High Speed Index Score',
        columnProp: 'highSpeedIndexScore',
        options: [],
				style: 'search-input search-input-hsis search-input-score-s'
      }, {
        name: 'Ped Injury Exposure Index Score',
        columnProp: 'pedInjuryExposureIndexScore',
        options: [],
				style: 'search-input search-input-pieis search-input-score-s'
      }, {
        name: 'Crash Rate Index',
        columnProp: 'crashRateIndex',
        options: [],
				style: 'search-input search-input-cri search-input-metric-s'
      }, {
        name: 'Kabco Crash Severity Index',
        columnProp: 'kabcoCrashSeverityIndex',
        options: [],
				style: 'search-input search-input-kcsi search-input-metric-s'
      }, {
        name: 'High Speed Index',
        columnProp: 'highSpeedIndex',
        options: [],
				style: 'search-input search-input-hsi search-input-metric-s'
      }, {
        name: 'Ped Injury Exposure Index',
        columnProp: 'pedInjuryExposureIndex',
        options: [],
				style: 'search-input search-input-piei search-input-metric-s'
      }
    ];

    this.dataSourceSafety.filterPredicate = ((data, filter) => {
      const a = !filter.zone_Group || data.zone_Group.toLowerCase().includes(filter.zone_Group);
      const b = !filter.corridor || data.corridor.toLowerCase().includes(filter.corridor);
      const c = !filter.percentHealth || data.percentHealth.toLowerCase().includes(filter.percentHealth);
      const d = !filter.missingData || data.missingData.toLowerCase().includes(filter.missingData);
      const e = !filter.crashRateIndexScore || data.crashRateIndexScore.toLowerCase().includes(filter.crashRateIndexScore);
      const f = !filter.kabcoCrashSeverityIndexScore || data.kabcoCrashSeverityIndexScore.toLowerCase().includes(filter.kabcoCrashSeverityIndexScore);
      const g = !filter.highSpeedIndexScore || data.highSpeedIndexScore.toLowerCase().includes(filter.highSpeedIndexScore);
      const h = !filter.pedInjuryExposureIndexScore || data.pedInjuryExposureIndexScore.toLowerCase().includes(filter.pedInjuryExposureIndexScore);
      const i = !filter.crashRateIndex || data.crashRateIndex.toLowerCase().includes(filter.crashRateIndex);
      const j = !filter.kabcoCrashSeverityIndex || !data.kabcoCrashSeverityIndex || data.modified.toLowerCase().includes(filter.kabcoCrashSeverityIndex);
      const k = !filter.highSpeedIndex || data.highSpeedIndex.toLowerCase().includes(filter.highSpeedIndex);
      const l = !filter.pedInjuryExposureIndex || data.pedInjuryExposureIndex.toLowerCase().includes(filter.pedInjuryExposureIndex);

      return a && b && c && d && e && f && g && h && i && j && k && l;
    }) as (HealthSafety, string) => boolean;

    this.formControlSafety = formBuilder.group({
      zone_Group: '',
      corridor: '',
      percentHealth: '',
      missingData: '',
      crashRateIndexScore: '',
      kabcoCrashSeverityIndexScore: '',
      highSpeedIndexScore: '',
      pedInjuryExposureIndexScore: '',
      crashRateIndex: '',
      kabcoCrashSeverityIndex: '',
      highSpeedIndex: '',
      pedInjuryExposureIndex: ''
    });
    this.formControlSafety.valueChanges.subscribe(value => {
      const filter = {...value,
        zone_Group: value.zone_Group.trim().toLowerCase(),
        corridor: value.corridor.trim().toLowerCase(),
        percentHealth: value.percentHealth.trim().toLowerCase(),
        missingData: value.missingData.trim().toLowerCase(),
        crashRateIndexScore: value.crashRateIndexScore.trim().toLowerCase(),
        kabcoCrashSeverityIndexScore: value.kabcoCrashSeverityIndexScore.trim().toLowerCase(),
        highSpeedIndexScore: value.commUptimeScore.trim().toLowerCase(),
        pedInjuryExposureIndexScore: value.cctvUptimeScore.trim().toLowerCase(),
        crashRateIndex: value.flashEventsScore.trim().toLowerCase(),
        kabcoCrashSeverityIndex: value.detectionUptime.trim().toLowerCase(),
        highSpeedIndex: value.pedDelay.trim().toLowerCase(),
        pedInjuryExposureIndex: value.commUptime.trim().toLowerCase()
      } as string;
      this.dataSourceMaintenance.filter = filter;
    });

  }



  ngOnInit(): void {
    this.toggleService.toggleValue.subscribe(value => {
      this.toggleValue = value;
    });
    this.currentTab = "Maintenance";
    this.titleService.setTitle("SigOpsMetrics - HealthMetrics - Maintenance");
    this.filterService.updateFilterErrorState(3);
  }


  private maintenancePaginator: MatPaginator;
  private operationsPaginator: MatPaginator;
  private safetyPaginator: MatPaginator;
  @ViewChild(MatPaginator) set matPaginator(mp: MatPaginator) {
    this.maintenancePaginator = mp;
    this.setDataSourceAttributes();
  }
  @ViewChild(MatPaginator) set matPaginator2(mp: MatPaginator) {
    this.operationsPaginator = mp;
    this.setDataSourceAttributes();
  }
  @ViewChild(MatPaginator) set matPaginator3(mp: MatPaginator) {
    this.safetyPaginator = mp;
    this.setDataSourceAttributes();
  }
  setDataSourceAttributes(){
    this.dataSourceMaintenance.paginator = this.maintenancePaginator;
    this.dataSourceOperations.paginator = this.operationsPaginator;
    this.dataSourceSafety.paginator = this.safetyPaginator;
  }
  ngAfterViewInit() {
    var currentDate = new Date();
    var startDate = (currentDate.getMonth() -1) + '-01-' + currentDate.getFullYear();
    var endDate = (currentDate.getMonth() +1) + '-02-' + currentDate.getFullYear();

    //Set the paginator before the data or performance really tanks
    this.setRegionStatus();

    var maintMetric = new Metrics();
    maintMetric.source = "main";
    maintMetric.level = "cor";
    maintMetric.interval = "mo";
    maintMetric.measure = "maint_plot";
    maintMetric.start = startDate;
    maintMetric.end = endDate;
    this.metricsService.getMetrics(maintMetric).subscribe(data => {
      this.dataSourceMaintenance.data = data
    });

    var opsMetric = new Metrics();
    opsMetric.source = "main";
    opsMetric.level = "cor";
    opsMetric.interval = "mo";
    opsMetric.measure = "ops_plot";
    opsMetric.start = startDate;
    opsMetric.end = endDate;
    this.metricsService.getMetrics(opsMetric).subscribe(data => this.dataSourceOperations.data = data);

    var safetyMetric = new Metrics();
    safetyMetric.source = "main";
    safetyMetric.level = "cor";
    safetyMetric.interval = "mo";
    safetyMetric.measure = "safety_plot";
    safetyMetric.start = startDate;
    safetyMetric.end = endDate;
    this.metricsService.getMetrics(safetyMetric).subscribe(data => {
      this.dataSourceSafety.data = data
    });
  }

  // Reset table filters
  resetFilters() {
    this.filterSelectListMaintenance.forEach((value, key) => {
      value.modelValue = undefined;
      this.formControlMaintenance.get(value.columnProp).setValue('');
    });
    this.dataSourceMaintenance.filter = "";

    this.filterSelectListOperations.forEach((value, key) => {
      value.modelValue = undefined;
      this.formControlOperations.get(value.columnProp).setValue('');
    });
    this.dataSourceOperations.filter = "";

    this.filterSelectListSafety.forEach((value, key) => {
      value.modelValue = undefined;
      this.formControlSafety.get(value.columnProp).setValue('');
    });
    this.dataSourceSafety.filter = "";
  }

  sortData(sort: Sort) {
  if(this.currentTab==="Maintenance"){
    const data = this.dataSourceMaintenance.data.slice();
    if (!sort.active || sort.direction === '') {
      this.sortedDataMaintenance = data;
      return;
    }

    this.sortedDataMaintenance = data.sort((a, b) => {
      const isAsc = sort.direction === 'asc';
      switch (sort.active) {
        case 'zone_Group': return this.compare(a.zone_Group, b.zone_Group, isAsc);
        case 'corridor': return this.compare(a.corridor, b.corridor, isAsc);
        case 'percent Health': return this.compare(a['percent Health'], b['percent Health'], isAsc);
        case 'missing Data': return this.compare(a['missing Data'], b['missing Data'], isAsc);
        case 'detection Uptime Score': return this.compare(a['detection Uptime Score'], b['detection Uptime Score'], isAsc);
        case 'ped Actuation Uptime Score': return this.compare(a['ped Actuation Uptime Score'], b['ped Actuation Uptime Score'], isAsc);
        case 'comm Uptime Score': return this.compare(a['comm Uptime Score'], b['comm Uptime Score'], isAsc);
        case 'cctv Uptime Score': return this.compare(a['cctv Uptime Score'], b['cctv Uptime Score'], isAsc);
        case 'flash Event Score': return this.compare(a['flash Events Score'], b['flash Events Score'], isAsc);
        case 'detection Uptime': return this.compare(a['detection Uptime'], b['detection Uptime'], isAsc);
        case 'ped Actuation Uptime ': return this.compare(a['ped Actuation Uptime'], b['ped Actuation Uptime'], isAsc);
        case 'comm Uptime': return this.compare(a['comm Uptime'], b['comm Uptime'], isAsc);
        case 'cctv Uptime': return this.compare(a['cctv Uptime'], b['cctv Uptime'], isAsc);
        case 'flash Events': return this.compare(a['flash Events'], b['flash Events'], isAsc);
        default: return 0;
      }
    });
    this.dataSourceMaintenance.data = this.sortedDataMaintenance;
  }
  else if(this.currentTab === "Operations"){
    const data2 = this.dataSourceOperations.data.slice();
    if (!sort.active || sort.direction === '') {
      this.sortedDataOperations = data2;
      return;
    }

    this.sortedDataOperations = data2.sort((a, b) => {
      const isAsc2 = sort.direction === 'asc';
      switch (sort.active) {
        case 'zone_Group': return this.compare(a.zone_Group, b.zone_Group, isAsc2);
        case 'corridor': return this.compare(a.corridor, b.corridor, isAsc2);
        case 'percent Health': return this.compare(a['percent Health'], b['percent Health'], isAsc2);
        case 'missing Data': return this.compare(a['missing Data'], b['missing Data'], isAsc2);
        case 'platoon Ratio Score': return this.compare(a['platoon Ratio Score'], b['platoon Ratio Score'], isAsc2);
        case 'ped Delay Score': return this.compare(a['ped Delay Score'], b['ped Delay Score'], isAsc2);
        case 'split Failures Score': return this.compare(a['split Failures Score'], b['split Failures Score'], isAsc2);
        case 'travel Time Index Score': return this.compare(a['travel Time Index Score'], b['travel Time Index Score'], isAsc2);
        case 'buffer Index Score': return this.compare(a['buffer Index Score'], b['buffer Index Score'], isAsc2);
        case 'platoon Ratio': return this.compare(a['platoon Ratio'], b['platoon Ratio'], isAsc2);
        case 'ped Delay': return this.compare(a['ped Delay'], b['ped Delay'], isAsc2);
        case 'split Failures': return this.compare(a['split Failures'], b['split Failures'], isAsc2);
        case 'travel Time Index': return this.compare(a['travel Time Index'], b['travel Time Index'], isAsc2);
        case 'buffer Index': return this.compare(a['buffer Index'], b['buffer Index'], isAsc2);
        default: return 0;
      }
    });
    this.dataSourceOperations.data = this.sortedDataOperations;
  }
  else if (this.currentTab === "Safety") {
    const data3 = this.dataSourceSafety.data.slice();
    if (!sort.active || sort.direction === '') {
      this.sortedDataSafety = data3;
      return;
    }

    this.sortedDataSafety = data3.sort((a, b) => {
      const isAsc3 = sort.direction === 'asc';
      switch (sort.active) {
        case 'zone_Group': return this.compare(a.zone_Group, b.zone_Group, isAsc3);
        case 'corridor': return this.compare(a.corridor, b.corridor, isAsc3);
        case 'percent Health': return this.compare(a['percent Health'], b['percent Health'], isAsc3);
        case 'missing Data': return this.compare(a['missing Data'], b['missing Data'], isAsc3);
        case 'crash Rate Index Score': return this.compare(a['crash Rate Index Score'], b['crash Rate Index Score'], isAsc3);
        case 'kabco Crash Severity Index Score': return this.compare(a['kabco Crash Severity Index Score'], b['kabco Crash Severity Index Score'], isAsc3);
        case 'high Speed Index Score': return this.compare(a['high Speed Index Score'], b['high Speed Index Score'], isAsc3);
        case 'ped Injury Exposure Index Score': return this.compare(a['ped Injury Exposure Index Score'], b['ped Injury Exposure Score'], isAsc3);
        case 'crash Rate Index': return this.compare(a['crash Rate Index'], b['crash Rate Index'], isAsc3);
        case 'kabco Crash Severity Index': return this.compare(a['kabco Crash Severity Index'], b['kabco Crash Severity Index'], isAsc3);
        case 'high Speed Index ': return this.compare(a['high Speed Index'], b['high Speed Index'], isAsc3);
        case 'ped Injury Exposure Index': return this.compare(a['ped Injury Exposure Index'], b['ped Injury Exposure Index'], isAsc3);
        default: return 0;
      }
    });
    this.dataSourceSafety.data = this.sortedDataSafety;
  }
}

  compare(a: number | string | Date, b: number | string | Date, isAsc: boolean) {
    return (a < b ? -1 : 1) * (isAsc ? 1 : -1);
  }

  compareDate(a: Date, b: Date, isAsc: boolean) {
    if (a == null && b == null) return 0;
    if (a == null) -1 * (isAsc ? 1 : -1);
    if (b == null) 1 * (isAsc ? 1 : -1);
    var val = (new Date(a).valueOf() - new Date(b).valueOf()) * (isAsc ? 1 : -1);
    //var val = isAsc ? +a - +b : +b - +a;
    return val;
  }

  compareNum(a: string, b: string, isAsc: boolean) {
    return (parseInt(a) < parseInt(b) ? -1 : 1) * (isAsc ? 1 : -1);
  }

  compareFloat(a: string, b: string, isAsc: boolean) {
    return (a.length == 0 ? -1 : parseFloat(a) < parseFloat(b) ? -1 : 1) * (isAsc ? 1 : -1);
  }

  tabChanged(tabChangeEvent: MatTabChangeEvent): void {
    this.currentTab = tabChangeEvent.tab.textLabel;
    this.titleService.setTitle("SigOpsMetrics - HealthMetrics - " + this.currentTab);
    setTimeout(()=>{
    switch(this.currentTab){
      case "Maintenance":
        !this.dataSourceMaintenance.paginator ? this.dataSourceMaintenance.paginator = this.maintenancePaginator : null;
        break;
      case "Operations":
        this.dataSourceOperations.paginator ? this.dataSourceOperations.paginator = this.operationsPaginator : null;
        break;
      case "Safety":
        this.dataSourceSafety.paginator ? this.dataSourceSafety.paginator = this.safetyPaginator : null;
    }
    })
  }

  evaluateFilter(data, key, filter, percent: boolean): any {
    var result = null;
    if (data && data[key]) {
      var number = Number.parseFloat(data[key]);
      if (percent) {
        result = (number * 100).toFixed(2).toString().includes(filter)
      } else {
        result = number.toFixed(2).toString().includes(filter)
      }
    }
    return result;
  }

  setRegionStatus(){
    var date = new Date();
    let dateString = date.getFullYear() + "-" + (date.getMonth() + 1) + "-01";
    this.metricsService.averagesForMonth('North', dateString).subscribe(res => this.northData = res.map(function(x) { return Math.round((x * 100)*1e2) / 1e2}));
    this.metricsService.averagesForMonth('Southeast', dateString).subscribe(res => this.southEastData = res.map(function(x) { return Math.round((x * 100)*1e2) / 1e2}));
    this.metricsService.averagesForMonth('Southwest', dateString).subscribe(res => this.southWestData = res.map(function(x) { return Math.round((x * 100)*1e2) / 1e2}));
    this.metricsService.averagesForMonth('Central Metro', dateString).subscribe(res => this.centralMetroData = res.map(function(x) { return Math.round((x * 100)*1e2) / 1e2}));
    this.metricsService.averagesForMonth('Western Metro', dateString).subscribe(res => this.westernMetroData = res.map(function(x) { return Math.round((x * 100)*1e2) / 1e2}));
    this.metricsService.averagesForMonth('Eastern Metro', dateString).subscribe(res => this.easternMetroData = res.map(function(x) { return Math.round((x * 100)*1e2) / 1e2}));
    this.metricsService.averagesForMonth('', dateString).subscribe(res => this.statewideData = res.map(function(x) { return Math.round((x * 100)*1e2) / 1e2}));
  }
}
