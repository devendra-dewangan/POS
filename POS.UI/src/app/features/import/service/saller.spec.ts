import { TestBed } from '@angular/core/testing';

import { Saller } from './saller';

describe('Saller', () => {
  let service: Saller;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Saller);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
