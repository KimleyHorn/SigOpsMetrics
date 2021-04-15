import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ScatterMapComponent } from './scatter-map.component';

describe('ScatterMapComponent', () => {
  let component: ScatterMapComponent;
  let fixture: ComponentFixture<ScatterMapComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ScatterMapComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ScatterMapComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
