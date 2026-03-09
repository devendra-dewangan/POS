import { TestBed } from '@angular/core/testing';

import { Buyer } from './buyer';

describe('Buyer', () => {
  let service: Buyer;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Buyer);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
