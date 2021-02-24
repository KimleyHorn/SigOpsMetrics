import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TeamsTasksComponent } from './teams-tasks.component';

describe('TeamsTasksComponent', () => {
  let component: TeamsTasksComponent;
  let fixture: ComponentFixture<TeamsTasksComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TeamsTasksComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TeamsTasksComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
