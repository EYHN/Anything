import React from 'react';
import { FixedSizeGrid as Grid, GridChildComponentProps, areEqual } from 'react-window';
import File from './File';
import useDomEvent, { useCustomEvent } from 'components/useDomEvent';
import useSelectController from 'components/Layout/GridLayout/useSelectController';
import { FrameRect, BoxSelectContainer } from 'components/useBoxSelectContainer';
import { IListDirectoryEntryFragment, IListDirectoryFragment } from 'api';
import { useSelection } from 'containers/Selection';

interface IGridLayoutProps {
  onOpen?: (path: string) => void;
  size: number;
  directory?: IListDirectoryFragment;
  height: number;
  width: number;
}

const RenderItem: React.FunctionComponent<GridChildComponentProps> = React.memo(({ columnIndex, rowIndex, style, data }) => {
  const { columnCount, columnWidth, rowHeight, entries, selection, setSelection, onOpen } = data;
  const index = rowIndex * columnCount + columnIndex;
  const entry = entries[index] as IListDirectoryEntryFragment;
  const focus = entry && selection && !!selection.find((i: string) => i === entry.path);

  const imgRef = React.useRef<HTMLImageElement>(null);
  const textRef = React.useRef<HTMLElement>(null);

  const handleOnMouseDown = React.useCallback((e: MouseEvent) => {
    e.stopPropagation();
    if(!entry) return;
    if (e.shiftKey && !!selection) {
      const start = entries.findIndex((e: any) => e.path === selection[0]);
      const end = index;
      const newselected = entries.slice(Math.min(start, end), Math.max(start, end) + 1).map((e: any) => e.path)
      if (end < start) newselected.reverse()
      setSelection(newselected);
    } else if (e.ctrlKey && !!selection) {
      if (focus) {
        setSelection(selection.concat().filter((path: string) => path !== entry.path));
      } else {
        setSelection([entry.path, ...selection]);
      }
    } else {
      setSelection([entry.path]);
    }
  }, [setSelection, selection, entry]);

  const handleOnDoubleClick = React.useCallback(() => {
    if (entry.__typename === 'Directory')
    onOpen(entry.path)
  }, [entry]);

  useDomEvent(imgRef, 'dblclick', handleOnDoubleClick);
  useDomEvent(textRef, 'dblclick', handleOnDoubleClick);
  useDomEvent(imgRef, 'mousedown', handleOnMouseDown);
  useDomEvent(textRef, 'mousedown', handleOnMouseDown);

  if (!entry) return <></>;

  return <File entry={entry} key={entry.path} width={columnWidth} height={rowHeight} focus={focus} style={style} imgRef={imgRef} textRef={textRef} />
}, areEqual)

const GridLayout: React.FunctionComponent<IGridLayoutProps> = ({ size, directory, onOpen, width, height }) => {

  if (!directory) return <></>;

  const gridOuterRef = React.useRef<HTMLElement>(null);
  const containerRef = React.useRef<HTMLDivElement>(null);

  const columnMinWidth = Math.max(595 + 150 - size, 100);
  const columnCount = Math.floor((width - 10) / columnMinWidth);
  const columnWidth = (width - 10) / columnCount;
  const rowHeight = 640 + 150 - size;
  const rowCount = Math.ceil(directory.entries.length / columnCount);

  const [selection, setSelection] = useSelection();
  const {unselectAll, onBoxSelectRect, onBoxSelectStart} = useSelectController(selection, setSelection, columnWidth, rowHeight, columnCount, rowCount, directory.entries);

  const handleOnMouseDown = React.useCallback((e: MouseEvent) => {
    if (e.button !== 0) return;

    if (e.shiftKey || e.ctrlKey) return
    unselectAll();
  }, [unselectAll]);
  useDomEvent(gridOuterRef, 'mousedown', handleOnMouseDown);

  useCustomEvent(gridOuterRef, 'box-select-start', onBoxSelectStart);
  
  const handleOnBoxSelectUpdate = React.useCallback((e: CustomEvent<FrameRect>) => {
    onBoxSelectRect(e.detail);
  }, [onBoxSelectRect]);
  useCustomEvent(gridOuterRef, 'box-select-update', handleOnBoxSelectUpdate);

  const handleOnOpen = React.useCallback((path: string) => {
    if (typeof onOpen === 'function') {
      onOpen(path);
    }
  }, [onOpen]);

  const handleItemKey = React.useCallback(({columnIndex, data, rowIndex}: {columnIndex: number, data: any, rowIndex: number}) => {
    const { columnCount, entries } = data;
    const index = rowIndex * columnCount + columnIndex;
    const entry = entries[index];
    return entry?.path || (columnIndex + ':' + rowIndex);
    
  }, []);

  const grid = React.useMemo(() => (
    <Grid
      style={{ overflowX: 'hidden' }}
      height={height}
      width={width}
      columnWidth={columnWidth}
      columnCount={columnCount}
      rowHeight={rowHeight}
      rowCount={rowCount}
      itemData={{ entries: directory.entries, columnCount, columnWidth, rowHeight, selection, setSelection, onOpen: handleOnOpen }}
      outerElementType={BoxSelectContainer}
      outerRef={gridOuterRef}
      itemKey={handleItemKey}
    >
      {RenderItem}
    </Grid>
  ), [directory.entries, height, width, columnWidth, columnCount, rowHeight, selection, setSelection, handleOnOpen])

  return <div style={{width: '100%', height: '100%', overflow: 'hidden', position: 'relative', userSelect: 'none'}} ref={containerRef}>
    {grid}
  </div>
}

export default GridLayout;