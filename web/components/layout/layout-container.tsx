import { IFileFragment } from 'api';
import { UIEvent, useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { IRect } from 'utils/rect';
import { ISize } from 'utils/size';
import BoxSelectContainer from './box-select-container';
import { LayoutManager, LayoutProps } from './interface';

export interface LayoutContainerProps<TLayoutManager extends LayoutManager<unknown>> {
  viewport: ISize;
  files: ReadonlyArray<IFileFragment>;
  layoutManager: TLayoutManager;
  selected: string[];
  onSelectedUpdate: (newSelected: string[]) => void;
  onRenderItem: (props: { file: IFileFragment; viewport: ISize }) => React.ReactNode;
}

function LayoutContainer<TLayoutManager extends LayoutManager<unknown>>({
  viewport,
  files,
  selected,
  layoutManager,
  onSelectedUpdate,
  onRenderItem,
}: LayoutContainerProps<TLayoutManager>) {
  const viewportRef = useRef<ISize>();
  const layoutPropsRef = useRef<LayoutProps>();
  const scrollPositionRef = useRef<{ scrollTop: number; scrollLeft: number }>();
  const layoutDataRef = useRef<ReturnType<typeof layoutManager.layout>>();
  const UpdateLayout = useCallback(
    (viewport: ISize, scrollPosition: { scrollTop: number; scrollLeft: number }, totalCount: number) => {
      const prevLayoutData = layoutDataRef.current;
      const prevLayoutProps = layoutPropsRef.current;
      const prevWindowRect = scrollPositionRef.current &&
        viewportRef.current && {
          top: scrollPositionRef.current.scrollTop,
          right: scrollPositionRef.current.scrollLeft + viewportRef.current.width,
          bottom: scrollPositionRef.current.scrollTop + viewportRef.current.height,
          left: scrollPositionRef.current.scrollLeft,
        };
      const layoutProps = { totalCount, viewport };
      const windowRect = {
        top: scrollPosition.scrollTop,
        right: scrollPosition.scrollLeft + viewport.width,
        bottom: scrollPosition.scrollTop + viewport.height,
        left: scrollPosition.scrollLeft,
      };

      viewportRef.current = viewport;
      layoutPropsRef.current = layoutProps;
      scrollPositionRef.current = scrollPosition;

      if (
        prevLayoutData &&
        prevLayoutProps &&
        prevWindowRect &&
        layoutManager.shouldUpdateLayout &&
        !layoutManager.shouldUpdateLayout({
          prevWindowRect,
          prevLayoutProps,
          prevLayoutData,
          newWindowRect: windowRect,
          newLayoutProps: layoutProps,
        })
      ) {
        return prevLayoutData;
      }

      return layoutManager.layout({ layoutProps, windowRect });
    },
    [layoutManager],
  );

  const [layoutData, setLayoutData] = useState<ReturnType<typeof layoutManager.layout>>(() =>
    UpdateLayout(viewport, { scrollTop: 0, scrollLeft: 0 }, files.length),
  );
  layoutDataRef.current = layoutData;

  const savedSelectedRef = useRef<string[]>([]);

  const handleSelectRect = useCallback(
    (rect: IRect) => {
      if (!layoutPropsRef.current) {
        return;
      }

      const selectingItems = layoutManager.select({ layoutProps: layoutPropsRef.current, layoutData, selectRect: rect }).items;

      const selectingKeys = selectingItems.map((item) => files[item.index].url);

      const newSelected = new Set([...savedSelectedRef.current, ...selectingKeys]);

      onSelectedUpdate(Array.from(newSelected));
    },
    [files, layoutManager, layoutData, onSelectedUpdate],
  );

  const handleSelectStart = useCallback(() => {
    savedSelectedRef.current = selected;
  }, [selected]);

  const handleScroll = useCallback(
    (e: UIEvent<HTMLDivElement>) => {
      setLayoutData(UpdateLayout(viewport, { scrollTop: e.currentTarget.scrollTop, scrollLeft: e.currentTarget.scrollLeft }, files.length));
    },
    [UpdateLayout, files.length, viewport],
  );

  // recalculate layout when viewport or files changed
  useEffect(() => {
    if (!scrollPositionRef.current) {
      return;
    }

    setLayoutData(UpdateLayout(viewport, scrollPositionRef.current, files.length));
  }, [viewport, files, UpdateLayout]);

  const content = useMemo(
    () => (
      <div style={{ height: layoutData.totalSize.height, width: layoutData.totalSize.width, position: 'relative', userSelect: 'none' }}>
        {layoutData.items.map((item) => {
          const file = files[item.index];
          return (
            <div
              key={item.index}
              style={{
                position: 'absolute',
                left: item.position.left,
                top: item.position.top,
                width: item.position.right - item.position.left,
                height: item.position.bottom - item.position.top,
              }}
            >
              {onRenderItem({
                file,
                viewport: { width: item.position.right - item.position.left, height: item.position.bottom - item.position.top },
              })}
            </div>
          );
        })}
      </div>
    ),
    [layoutData, files, onRenderItem],
  );

  return (
    <BoxSelectContainer
      style={{ width: viewport.width, height: viewport.height, overflow: 'auto' }}
      onSelectStart={handleSelectStart}
      onSelectRect={handleSelectRect}
      onScroll={handleScroll}
    >
      {content}
    </BoxSelectContainer>
  );
}

export default LayoutContainer;
