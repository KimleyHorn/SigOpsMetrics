import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HealthMetricsComponent } from './health-metrics.component';

describe('HealthMetricsComponent', () => {
  let component: HealthMetricsComponent;
  let fixture: ComponentFixture<HealthMetricsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ HealthMetricsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(HealthMetricsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
