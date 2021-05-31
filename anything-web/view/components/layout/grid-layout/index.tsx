import React from 'react';
import { FixedSizeGrid as Grid, GridChildComponentProps, areEqual } from 'react-window';
import File from './file';
import useDomEvent, { useCustomEvent } from 'components/use-dom-event';
import useSelectController from 'components/layout/grid-layout/use-select-controller';
import { FrameRect, BoxSelectContainer } from 'components/use-box-select-container';
import { IFileFragment } from 'api';
import { useSelection } from 'containers/selection';

interface IGridLayoutProps {
  onOpen?: (file: IFileFragment) => void;
  size: number;
  files: ReadonlyArray<IFileFragment>;
  height: number;
  width: number;
}

const RenderItem: React.FunctionComponent<GridChildComponentProps> = ({ columnIndex, rowIndex, style, data }) => {
  const { columnCount, columnWidth, rowHeight, entries, selection, setSelection, onOpen } = data;
  const index = rowIndex * columnCount + columnIndex;
  const entry = entries[index] as IFileFragment;
  const focus = entry && selection && !!selection.find((i: string) => i === entry.url);

  const imgRef = React.useRef<HTMLImageElement>(null);
  const textRef = React.useRef<HTMLElement>(null);

  const handleOnMouseDown = React.useCallback(
    (e: MouseEvent) => {
      e.stopPropagation();
      if (!entry) return;
      if (e.shiftKey && !!selection) {
        const start = entries.findIndex((e: IFileFragment) => e.url === selection[0]);
        const end = index;
        const newselected = entries.slice(Math.min(start, end), Math.max(start, end) + 1).map((e: IFileFragment) => e.url);
        if (end < start) newselected.reverse();
        setSelection(newselected);
      } else if (e.ctrlKey && !!selection) {
        if (focus) {
          setSelection(selection.concat().filter((url: string) => url !== entry.url));
        } else {
          setSelection([entry.url, ...selection]);
        }
      } else {
        setSelection([entry.url]);
      }
    },
    [setSelection, selection, entry],
  );

  const handleOnDoubleClick = React.useCallback(() => {
    if (entry.__typename === 'Directory') onOpen(entry);
  }, [entry]);

  useDomEvent(imgRef, 'dblclick', handleOnDoubleClick);
  useDomEvent(textRef, 'dblclick', handleOnDoubleClick);
  useDomEvent(imgRef, 'mousedown', handleOnMouseDown);
  useDomEvent(textRef, 'mousedown', handleOnMouseDown);

  if (!entry) return <></>;

  return (
    <File
      file={entry}
      key={entry.url}
      width={columnWidth}
      height={rowHeight}
      focus={focus}
      style={style}
      imgRef={imgRef}
      textRef={textRef}
    />
  );
};

const RenderItemMemo = React.memo(RenderItem, areEqual);

const GridLayout: React.FunctionComponent<IGridLayoutProps> = ({ size, files, onOpen, width, height }) => {
  const gridOuterRef = React.useRef<HTMLElement>(null);
  const containerRef = React.useRef<HTMLDivElement>(null);

  const columnMinWidth = Math.max(595 + 150 - size, 100);
  const columnCount = Math.floor((width - 10) / columnMinWidth);
  const columnWidth = (width - 10) / columnCount;
  const rowHeight = 640 + 150 - size;
  const rowCount = Math.ceil(files.length / columnCount);

  const [selection, setSelection] = useSelection();
  const { unselectAll, onBoxSelectRect, onBoxSelectStart } = useSelectController(
    selection,
    setSelection,
    columnWidth,
    rowHeight,
    columnCount,
    rowCount,
    files,
  );

  const handleOnMouseDown = React.useCallback(
    (e: MouseEvent) => {
      if (e.button !== 0) return;

      if (e.shiftKey || e.ctrlKey) return;
      unselectAll();
    },
    [unselectAll],
  );
  useDomEvent(gridOuterRef, 'mousedown', handleOnMouseDown);

  useCustomEvent(gridOuterRef, 'box-select-start', onBoxSelectStart);

  const handleOnBoxSelectUpdate = React.useCallback(
    (e: CustomEvent<FrameRect>) => {
      onBoxSelectRect(e.detail);
    },
    [onBoxSelectRect],
  );
  useCustomEvent(gridOuterRef, 'box-select-update', handleOnBoxSelectUpdate);

  const handleOnOpen = React.useCallback(
    (file: IFileFragment) => {
      if (typeof onOpen === 'function') {
        onOpen(file);
      }
    },
    [onOpen],
  );

  const handleItemKey = React.useCallback(
    ({
      columnIndex,
      data,
      rowIndex,
    }: {
      columnIndex: number;
      data: { columnCount: number; entries: IFileFragment[] };
      rowIndex: number;
    }) => {
      const { columnCount, entries } = data;
      const index = rowIndex * columnCount + columnIndex;
      const entry = entries[index];
      return entry?.url || columnIndex + ':' + rowIndex;
    },
    [],
  );

  const grid = React.useMemo(
    () => (
      <Grid
        style={{ overflowX: 'hidden' }}
        height={height}
        width={width}
        columnWidth={columnWidth}
        columnCount={columnCount}
        rowHeight={rowHeight}
        rowCount={rowCount}
        itemData={{ entries: files, columnCount, columnWidth, rowHeight, selection, setSelection, onOpen: handleOnOpen }}
        outerElementType={BoxSelectContainer}
        outerRef={gridOuterRef}
        itemKey={handleItemKey}
      >
        {RenderItemMemo}
      </Grid>
    ),
    [files, height, width, columnWidth, columnCount, rowHeight, selection, setSelection, handleOnOpen],
  );

  return (
    <div style={{ width: '100%', height: '100%', overflow: 'hidden', position: 'relative', userSelect: 'none' }} ref={containerRef}>
      {grid}
    </div>
  );
};

export default GridLayout;
