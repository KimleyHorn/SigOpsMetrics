import { TestBed } from '@angular/core/testing';

import { ChartToggleService } from './chart-toggle.service';

describe('ChartToggleService', () => {
  let service: ChartToggleService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ChartToggleService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
