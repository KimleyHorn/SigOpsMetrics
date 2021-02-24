import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RampMetersComponent } from './ramp-meters.component';

describe('RampMetersComponent', () => {
  let component: RampMetersComponent;
  let fixture: ComponentFixture<RampMetersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RampMetersComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RampMetersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
