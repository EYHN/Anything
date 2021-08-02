import { LayoutManager, LayoutItem } from '../interface';

const size = 600;
const overscan = 0;

interface GridLayoutHintData {
  columnWidth: number;
  columnCount: number;
  rowHeight: number;
  rowCount: number;
}

const GridLayout: LayoutManager<GridLayoutHintData> = {
  layout: ({ layoutProps, windowRect }) => {
    console.log('layout');
    const { viewport, totalCount } = layoutProps;

    const columnMinWidth = Math.max(595 + 150 - size, 100);
    const columnCount = Math.floor((viewport.width - 10) / columnMinWidth);
    const columnWidth = (viewport.width - 10) / columnCount;

    const rowHeight = 640 + 150 - size;
    const rowCount = Math.ceil(totalCount / columnCount);

    const rowBegin = Math.max(Math.floor(windowRect.top / rowHeight) - overscan, 0);
    const rowEnd = Math.min(Math.ceil(windowRect.bottom / rowHeight) + overscan, totalCount);

    const items: LayoutItem[] = [];
    for (let row = rowBegin; row < rowEnd; row++) {
      for (let column = 0; column < columnCount; column++) {
        const index = row * columnCount + column;

        if (index >= totalCount) {
          break;
        }

        items.push({
          index,
          position: { left: column * columnWidth, top: row * rowHeight, right: (column + 1) * columnWidth, bottom: (row + 1) * rowHeight },
        });
      }
    }

    return {
      items,
      totalSize: { width: columnCount * columnWidth, height: rowCount * rowHeight },
      hint: { columnCount, columnWidth, rowHeight, rowCount },
    };
  },
  select: () => {
    return { items: [] };
  },
};

export default GridLayout;
