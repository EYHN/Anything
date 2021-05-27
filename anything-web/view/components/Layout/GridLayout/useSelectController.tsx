import React from 'react';
import { FrameRect } from 'components/useBoxSelectContainer';
import isEqual from 'lodash-es/isEqual';
import { IFileFragment } from 'api';

function calcIndexFromFrame(frame: FrameRect, columnWidth: number, rowHeight: number) {
  const margin = 20;

  const leftx = Math.min(frame.x1, frame.x2);
  const rightx = Math.max(frame.x1, frame.x2);

  const topy = Math.min(frame.y1, frame.y2);
  const bottomy = Math.max(frame.y1, frame.y2);

  const columnIndex1 = Math.floor((leftx + margin) / columnWidth);
  const columnIndex2 = Math.floor((rightx - margin) / columnWidth + 1);

  const rowHeight1 = Math.floor((topy + margin) / rowHeight);
  const rowHeight2 = Math.floor((bottomy - margin) / rowHeight + 1);
  return [columnIndex1, columnIndex2, rowHeight1, rowHeight2];
}

export default function useSelectController(
  selected: string[],
  setSelected: (selected: string[]) => void,
  columnWidth: number,
  rowHeight: number,
  columnCount: number,
  rowCount: number,
  entries: ReadonlyArray<IFileFragment>,
) {
  const savedSelectedRef = React.useRef<string[]>([]);

  const unselectAll = React.useCallback(() => {
    setSelected([]);
  }, []);

  const onBoxSelectStart = React.useCallback(() => {
    savedSelectedRef.current = selected;
  }, [selected]);

  const onBoxSelectRect = React.useCallback(
    (rect: FrameRect) => {
      const [columnIndex1, columnIndex2, rowHeight1, rowHeight2] = calcIndexFromFrame(rect, columnWidth, rowHeight);

      const newSelected = [...savedSelectedRef.current];
      for (let columnIndex = columnIndex1; columnIndex < columnIndex2; columnIndex++) {
        for (let rowIndex = rowHeight1; rowIndex < rowHeight2; rowIndex++) {
          if (columnIndex >= columnCount || columnIndex < 0 || rowIndex >= rowCount || rowIndex < 0) continue;
          const index = rowIndex * columnCount + columnIndex;
          if (entries[index]) newSelected.push(entries[index].url);
        }
      }
      if (!isEqual(newSelected, selected)) setSelected(newSelected);
    },
    [selected, columnWidth, rowHeight, columnCount, rowCount],
  );

  return { onBoxSelectStart, onBoxSelectRect, unselectAll };
}

export type SelectController = ReturnType<typeof useSelectController>;
