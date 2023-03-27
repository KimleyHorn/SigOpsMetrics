import { ComponentFixture, TestBed } from "@angular/core/testing";

import { SummaryTrendComponent } from "./summary-trend.component";

describe("SummaryTrendComponent", () => {
  let component: SummaryTrendComponent;
  let fixture: ComponentFixture<SummaryTrendComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SummaryTrendComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SummaryTrendComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
