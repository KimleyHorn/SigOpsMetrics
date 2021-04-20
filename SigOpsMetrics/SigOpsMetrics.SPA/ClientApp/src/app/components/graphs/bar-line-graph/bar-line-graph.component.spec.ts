import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BarLineGraphComponent } from './bar-line-graph.component';

describe('BarLineGraphComponent', () => {
  let component: BarLineGraphComponent;
  let fixture: ComponentFixture<BarLineGraphComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BarLineGraphComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(BarLineGraphComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
