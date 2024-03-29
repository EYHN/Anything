import { IRect } from 'utils/rect';
import shallowEqual from 'utils/shallow-equal';
import { ISize } from 'utils/size';
import { LayoutItem, LayoutManager, LayoutProps } from '../types';

const horizontalPadding = 36;
const verticalPadding = 12;

const rectToGrid = (containerSize: ISize, totalCount: number, rect: IRect) => {
  const containerWidth = containerSize.width - horizontalPadding * 2;

  const columnMinSpace = 16;
  const columnWidth = 123;
  const columnCount = Math.max(Math.floor(containerWidth / (columnWidth + columnMinSpace)), 1);
  const columnSpace = Math.round(columnCount > 1 ? (containerWidth - columnWidth * columnCount) / (columnCount - 1) : 0);

  const columnBegin = Math.max(Math.floor((rect.left - horizontalPadding + columnSpace) / (columnWidth + columnSpace)), 0);
  const columnEnd = Math.min(Math.floor((rect.right - horizontalPadding) / (columnWidth + columnSpace)), columnCount - 1);

  const rowSpace = 16;
  const rowHeight = 200;
  const rowCount = Math.ceil(totalCount / columnCount);

  const rowBegin = Math.max(Math.floor((rect.top - verticalPadding + rowSpace) / (rowHeight + rowSpace)), 0);
  const rowEnd = Math.min(Math.floor((rect.bottom - verticalPadding) / (rowHeight + rowSpace)), rowCount - 1);

  const itemCount =
    Math.max(columnEnd - columnBegin + 1, 0) * Math.max(rowEnd - rowBegin, 0) +
    (rowEnd > rowBegin - 1
      ? Math.max(
          Math.min(rowEnd == rowCount - 1 && totalCount % columnCount != 0 ? (totalCount % columnCount) - 1 : columnCount - 1, columnEnd) -
            columnBegin +
            1,
          0,
        )
      : 0);

  return {
    columnCount,
    columnWidth,
    columnSpace,
    columnBegin,
    columnEnd,
    rowCount,
    rowSpace,
    rowHeight,
    rowBegin,
    rowEnd,
    itemCount,
  };
};

const calculateLayoutState = (layoutProps: LayoutProps) => {
  const { containerSize, totalCount, windowRect } = layoutProps;

  const { columnCount, columnWidth, columnSpace, columnBegin, columnEnd, rowCount, rowSpace, rowHeight, rowBegin, rowEnd } = rectToGrid(
    containerSize,
    totalCount,
    windowRect,
  );

  return {
    horizontalPadding,
    verticalPadding,
    columnCount,
    columnWidth,
    columnSpace,
    columnBegin,
    columnEnd,
    rowSpace,
    rowHeight,
    rowBegin,
    rowEnd,
    totalCount,
    sceneWidth: containerSize.width,
    sceneHeight: rowHeight * rowCount + rowSpace * (rowCount - 1) + verticalPadding * 2,
  };
};

export type GridLayoutManagerHint = ReturnType<typeof calculateLayoutState>;

const GridLayoutManager: LayoutManager<GridLayoutManagerHint> = {
  layout: ({ layoutProps, prevLayoutData }) => {
    const layoutState = calculateLayoutState(layoutProps);
    if (prevLayoutData && shallowEqual(layoutState, prevLayoutData.hint)) {
      return false;
    }
    const {
      horizontalPadding,
      verticalPadding,
      columnCount,
      columnWidth,
      columnBegin,
      columnEnd,
      rowSpace,
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
      for (let column = columnBegin; column <= columnEnd; column++) {
        const index = row * columnCount + column;

        if (index >= totalCount) {
          break;
        }

        const left = column * columnWidth + columnSpace * column + horizontalPadding;
        const top = row * (rowHeight + rowSpace) + verticalPadding;

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
  select: ({ selectProps, layoutProps }) => {
    const { selectRect } = selectProps;
    const { containerSize, totalCount } = layoutProps;

    const { columnCount, columnBegin, columnEnd, rowBegin, rowEnd } = rectToGrid(containerSize, totalCount, selectRect);

    const items: number[] = [];
    for (let row = rowBegin; row <= rowEnd; row++) {
      for (let column = columnBegin; column <= columnEnd; column++) {
        const index = row * columnCount + column;

        if (index >= totalCount) {
          break;
        }

        items.push(index);
      }
    }

    return {
      items,
    };
  },
};

export default GridLayoutManager;
