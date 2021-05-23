import { SPECS } from './spec';

/**
 * file size
 * @param bytes
 * @param fixed
 * @param spec
 */
export default function (bytes: number, fixed = 1, spec?: string): string {
  bytes = Math.abs(bytes);

  // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
  const { radix, unit } = SPECS[spec!] || SPECS['jedec'];

  let loop = 0;

  // calculate
  while (bytes >= radix) {
    bytes /= radix;
    ++loop;
  }
  return `${bytes.toFixed(fixed)} ${unit[loop]}`;
}
