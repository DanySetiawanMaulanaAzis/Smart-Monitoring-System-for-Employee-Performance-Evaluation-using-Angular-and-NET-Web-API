import { TestBed } from '@angular/core/testing';

import { Engineer } from './engineer';

describe('Engineer', () => {
  let service: Engineer;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Engineer);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
