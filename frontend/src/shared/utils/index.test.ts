import { describe, it, expect } from 'vitest';
import { cn } from './index';

describe('cn utility', () => {
  it('merges tailwind classes correctly', () => {
    expect(cn('bg-red-500', 'text-white')).toBe('bg-red-500 text-white');
  });

  it('handles conditional classes', () => {
    expect(cn('bg-red-500', false && 'text-white', true && 'p-4')).toBe('bg-red-500 p-4');
  });

  it('resolves tailwind conflicts', () => {
    // text-sm should be overridden by text-lg
    expect(cn('text-sm px-2', 'text-lg')).toBe('px-2 text-lg');
  });
});
