import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BarLineLineGraphComponent } from './bar-line-line-graph.component';

describe('BarLineLineGraphComponent', () => {
  let component: BarLineLineGraphComponent;
  let fixture: ComponentFixture<BarLineLineGraphComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BarLineLineGraphComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(BarLineLineGraphComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
