import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ThroughputGraphComponent } from './throughput-graph.component';

describe('ThroughputGraphComponent', () => {
  let component: ThroughputGraphComponent;
  let fixture: ComponentFixture<ThroughputGraphComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ThroughputGraphComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ThroughputGraphComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
