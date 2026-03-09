import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Saller } from './saller';

describe('Saller', () => {
  let component: Saller;
  let fixture: ComponentFixture<Saller>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [Saller],
    }).compileComponents();

    fixture = TestBed.createComponent(Saller);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
