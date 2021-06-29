import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GraphNoCardDashboardComponent } from './graph-no-card-dashboard.component';

describe('GraphNoCardDashboardComponent', () => {
  let component: GraphNoCardDashboardComponent;
  let fixture: ComponentFixture<GraphNoCardDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ GraphNoCardDashboardComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(GraphNoCardDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
