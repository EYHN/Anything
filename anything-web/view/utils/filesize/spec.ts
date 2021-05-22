type SPEC = {
  readonly radix: number;
  readonly unit: string[];
};

const jedec = { radix: 1024, unit: ['B', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'] };

export const SPECS: Record<string, SPEC> = {
  jedec,
};