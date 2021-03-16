import { Component, OnInit } from '@angular/core';
import { ChartToggleService } from 'src/app/components/toggles/chart-toggle/chart-toggle.service';

@Component({
  selector: 'app-operations',
  templateUrl: './operations.component.html',
  styleUrls: ['./operations.component.css']
})
export class OperationsComponent implements OnInit {
  toggleValue: string;

  constructor(private toggleService: ChartToggleService) { }

  ngOnInit(): void {
    this.toggleService.toggleValue.subscribe(value => {
      this.toggleValue = value;
    });
  }

}
