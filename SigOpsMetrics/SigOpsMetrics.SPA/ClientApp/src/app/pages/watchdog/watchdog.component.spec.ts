import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WatchdogComponent } from './watchdog.component';

describe('WatchdogComponent', () => {
  let component: WatchdogComponent;
  let fixture: ComponentFixture<WatchdogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ WatchdogComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(WatchdogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
