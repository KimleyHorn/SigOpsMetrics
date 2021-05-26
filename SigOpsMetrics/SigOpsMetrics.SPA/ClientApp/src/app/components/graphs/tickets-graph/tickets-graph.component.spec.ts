import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TicketsGraphComponent } from './tickets-graph.component';

describe('TicketsGraphComponent', () => {
  let component: TicketsGraphComponent;
  let fixture: ComponentFixture<TicketsGraphComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TicketsGraphComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TicketsGraphComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
