import { TestBed } from '@angular/core/testing';

import { MetricSelectService } from './metric-select.service';

describe('MetricSelectService', () => {
  let service: MetricSelectService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MetricSelectService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
