import { IRect } from 'utils/rect';
import { LayoutItem, LayoutManager, LayoutProps } from '../layout';
import shallowEqual from 'utils/shallow-equal';

const overscan = 2;
const horizontalPadding = 36;
const verticalPadding = 12;

const calculateLayoutState = (layoutProps: LayoutProps, windowRect: IRect) => {
  const { containerSize, totalCount } = layoutProps;

  const containerWidth = containerSize.width - horizontalPadding * 2;

  const columnMinSpace = 8;
  const columnWidth = 123;
  const columnCount = Math.max(Math.floor(containerWidth / (columnWidth + columnMinSpace)), 1);
  const columnSpace = columnCount > 1 ? (containerWidth - columnWidth * columnCount) / (columnCount - 1) : 0;

  const rowHeight = 180;
  const rowCount = Math.ceil(totalCount / columnCount);

  const rowBegin = Math.max(Math.floor(windowRect.top / rowHeight) - overscan, 0);
  const rowEnd = Math.min(Math.ceil(windowRect.bottom / rowHeight) + overscan, rowCount - 1);

  return {
    horizontalPadding,
    verticalPadding,
    columnCount,
    columnWidth,
    rowHeight,
    rowBegin,
    rowEnd,
    totalCount,
    columnSpace,
    sceneWidth: containerSize.width,
    sceneHeight: rowHeight * rowCount + verticalPadding * 2,
  };
};

const GridLayoutManager: LayoutManager<ReturnType<typeof calculateLayoutState>> = {
  layout: ({ layoutProps, windowRect }) => {
    const layoutState = calculateLayoutState(layoutProps, windowRect);
    const {
      horizontalPadding,
      verticalPadding,
      columnCount,
      columnWidth,
      rowHeight,
      rowBegin,
      rowEnd,
      totalCount,
      columnSpace,
      sceneWidth,
      sceneHeight,
    } = layoutState;

    const items: LayoutItem[] = [];
    for (let row = rowBegin; row <= rowEnd; row++) {
      for (let column = 0; column < columnCount; column++) {
        const index = row * columnCount + column;

        if (index >= totalCount) {
          break;
        }

        const left = column * columnWidth + columnSpace * column + horizontalPadding;
        const top = row * rowHeight + verticalPadding;

        items.push({
          index,
          position: {
            left,
            top: top,
            right: left + columnWidth,
            bottom: top + rowHeight,
          },
        });
      }
    }

    return {
      items,
      sceneSize: { width: sceneWidth, height: sceneHeight },
      hint: layoutState,
    };
  },
  select: () => {
    return { items: [] };
  },
  shouldUpdateLayout: ({ prevLayoutData, newLayoutProps: layoutProps, newWindowRect: windowRect }) => {
    const layoutState = calculateLayoutState(layoutProps, windowRect);
    return !shallowEqual(layoutState, prevLayoutData.hint);
  },
};

export default GridLayoutManager;
