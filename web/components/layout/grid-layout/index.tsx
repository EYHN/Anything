import React, { memo } from 'react';
import File from './file';
import { IFileFragment } from 'api';
import { useSelection } from 'containers/selection';
import LayoutContainer from '../layout-container';
import GridLayoutManager from './layout-manager';
import { ISize } from 'utils/size';

interface IGridLayoutProps {
  onOpen?: (file: IFileFragment) => void;
  size: number;
  files: ReadonlyArray<IFileFragment>;
  viewport: ISize;
}

interface IGridItemProps {
  file: IFileFragment;
  viewport: ISize;
}

const RenderItem = memo<IGridItemProps>(({ file, viewport }) => {
  // const { columnCount, columnWidth, rowHeight, entries, selection, setSelection, onOpen } = data;
  // const index = rowIndex * columnCount + columnIndex;
  // const entry = entries[index] as IFileFragment;
  // const focus = entry && selection && !!selection.find((i: string) => i === entry.url);

  const imgRef = React.useRef<HTMLImageElement>(null);
  const textRef = React.useRef<HTMLElement>(null);

  // const handleOnMouseDown = React.useCallback(
  //   (e: MouseEvent) => {
  //     e.stopPropagation();
  //     if (!entry) return;
  //     if (e.shiftKey && !!selection) {
  //       const start = entries.findIndex((e: IFileFragment) => e.url === selection[0]);
  //       const end = index;
  //       const newselected = entries.slice(Math.min(start, end), Math.max(start, end) + 1).map((e: IFileFragment) => e.url);
  //       if (end < start) newselected.reverse();
  //       setSelection(newselected);
  //     } else if (e.ctrlKey && !!selection) {
  //       if (focus) {
  //         setSelection(selection.concat().filter((url: string) => url !== entry.url));
  //       } else {
  //         setSelection([entry.url, ...selection]);
  //       }
  //     } else {
  //       setSelection([entry.url]);
  //     }
  //   },
  //   [setSelection, selection, entry],
  // );

  // const handleOnDoubleClick = React.useCallback(() => {
  //   if (entry.__typename === 'Directory') onOpen(entry);
  // }, [entry]);

  // useDomEvent(imgRef, 'dblclick', handleOnDoubleClick);
  // useDomEvent(textRef, 'dblclick', handleOnDoubleClick);
  // useDomEvent(imgRef, 'mousedown', handleOnMouseDown);
  // useDomEvent(textRef, 'mousedown', handleOnMouseDown);

  return (
    <File
      file={file}
      key={file.url}
      width={viewport.width}
      height={viewport.height}
      focus={false}
      imgRef={imgRef}
      textRef={textRef}
      style={{ display: 'inline-block' }}
    />
  );
});

RenderItem.displayName = 'memo(RenderItem)';

const GridLayout: React.FunctionComponent<IGridLayoutProps> = ({ files, viewport }) => {
  // const containerRef = React.useRef<HTMLDivElement>(null);

  // const columnMinWidth = Math.max(595 + 150 - size, 100);
  // const columnCount = Math.floor((width - 10) / columnMinWidth);
  // const columnWidth = (width - 10) / columnCount;
  // const rowHeight = 640 + 150 - size;
  // const rowCount = Math.ceil(files.length / columnCount);

  const [selection, setSelection] = useSelection();
  // const { unselectAll, onBoxSelectRect, onBoxSelectStart } = useSelectController(
  //   selection,
  //   setSelection,
  //   columnWidth,
  //   rowHeight,
  //   columnCount,
  //   rowCount,
  //   files,
  // );

  // const [scrollTop, setScrollTop] = React.useState(0);

  // const handleOnScroll = React.useCallback((e: Event) => {
  //   setScrollTop((e.target as HTMLElement).scrollTop);
  // }, []);
  // useDomEvent(containerRef, 'scroll', handleOnScroll);

  // const row = useVirtualWindow({ windowOffset: scrollTop, windowSize: height, itemSize: rowHeight, itemCount: rowCount, overscan: 0 });

  // const handleOnMouseDown = React.useCallback(
  //   (e: MouseEvent) => {
  //     if (e.button !== 0) return;

  //     if (e.shiftKey || e.ctrlKey) return;
  //     unselectAll();
  //   },
  //   [unselectAll],
  // );
  // useDomEvent(containerRef, 'mousedown', handleOnMouseDown);

  // useCustomEvent(containerRef, 'box-select-start', onBoxSelectStart);

  // const handleOnBoxSelectUpdate = React.useCallback(
  //   (e: CustomEvent<FrameRect>) => {
  //     onBoxSelectRect(e.detail);
  //   },
  //   [onBoxSelectRect],
  // );
  // useCustomEvent(containerRef, 'box-select-update', handleOnBoxSelectUpdate);

  // const handleOnOpen = React.useCallback(
  //   (file: IFileFragment) => {
  //     if (typeof onOpen === 'function') {
  //       onOpen(file);
  //     }
  //   },
  //   [onOpen],
  // );

  // const handleItemKey = React.useCallback(
  //   ({
  //     columnIndex,
  //     data,
  //     rowIndex,
  //   }: {
  //     columnIndex: number;
  //     data: { columnCount: number; entries: IFileFragment[] };
  //     rowIndex: number;
  //   }) => {
  //     const { columnCount, entries } = data;
  //     const index = rowIndex * columnCount + columnIndex;
  //     const entry = entries[index];
  //     return entry?.url || columnIndex + ':' + rowIndex;
  //   },
  //   [],
  // );

  // const grid = React.useMemo(
  //   () => (
  //     <Grid
  //       style={{ overflowX: 'hidden' }}
  //       height={height}
  //       width={width}
  //       columnWidth={columnWidth}
  //       columnCount={columnCount}
  //       rowHeight={rowHeight}
  //       rowCount={rowCount}
  //       itemData={{ entries: files, columnCount, columnWidth, rowHeight, selection, setSelection, onOpen: handleOnOpen }}
  //       outerElementType={BoxSelectContainer}
  //       outerRef={gridOuterRef}
  //       itemKey={handleItemKey}
  //     >
  //       {RenderItemMemo}
  //     </Grid>
  //   ),
  //   [files, height, width, columnWidth, columnCount, rowHeight, selection, setSelection, handleOnOpen],
  // );

  // const content = useMemo(
  //   () => (
  //     <div style={{ marginTop: row.begin, marginBottom: row.total - row.end }}>
  //       {row.items.map((item) => {
  //         const columns = [];
  //         for (let col = 0; col < columnCount; col++) {
  //           const file = files[item.index * columnCount + col];
  //           columns.push(<RenderItem data={file} width={columnWidth} height={rowHeight} />);
  //         }
  //         return (
  //           <div key={item.index} style={{ height: item.size, whiteSpace: 'nowrap' }}>
  //             {columns}
  //           </div>
  //         );
  //       })}
  //     </div>
  //   ),
  //   [row, columnWidth, rowHeight, files],
  // );

  return (
    <LayoutContainer
      layoutManager={GridLayoutManager}
      viewport={viewport}
      files={files}
      selected={selection}
      onSelectedUpdate={setSelection}
      onRenderItem={({ file, viewport }) => <RenderItem file={file} viewport={viewport} />}
    />
  );
};

export default GridLayout;
