import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LineBarGraphComponent } from './line-bar-graph.component';

describe('LineBarGraphComponent', () => {
  let component: LineBarGraphComponent;
  let fixture: ComponentFixture<LineBarGraphComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ LineBarGraphComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(LineBarGraphComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
