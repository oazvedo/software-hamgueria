import { clsx, type ClassValue } from 'clsx'
import { twMerge } from 'tailwind-merge'

/** Merge Tailwind classes, resolving conflicts (used by shadcn primitives too). */
export function cn(...inputs: ClassValue[]): string {
  return twMerge(clsx(inputs))
}
