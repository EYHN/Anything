import { useMemo } from 'react';

export interface VirtualWindowOptions {
  windowOffset: number;
  windowSize: number;
  itemCount: number;
  itemSize: number;
  overscan?: number;
}

export default function useVirtualWindow({ windowOffset, windowSize, itemSize, itemCount, overscan = 1 }: VirtualWindowOptions) {
  const windowBegin = windowOffset;
  const windowEnd = windowOffset + windowSize;
  const itemBegin = Math.max(Math.floor(windowBegin / itemSize) - overscan, 0);
  const itemEnd = Math.min(Math.ceil(windowEnd / itemSize) + overscan, itemCount);

  const items = useMemo(() => {
    const items = [];
    for (let i = itemBegin; i < itemEnd; i += 1) {
      items[i] = {
        index: i,
        size: itemSize,
        offset: itemSize * i,
      };
    }

    return items;
  }, [itemSize, itemBegin, itemEnd]);

  return useMemo(
    () => ({ items, total: itemCount * itemSize, begin: itemBegin * itemSize, end: itemEnd * itemSize }),
    [items, itemCount, itemSize, itemBegin, itemEnd],
  );
}
