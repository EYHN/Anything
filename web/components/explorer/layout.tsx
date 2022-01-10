import styled from '@emotion/styled';

import React, { memo, useCallback, useEffect, useMemo, useRef, useState } from 'react';
import useElementSize from 'components/use-element-size';

import { IRect, rectHasIntersection, rectSize } from 'utils/rect';
import { ISize } from 'utils/size';
import useBoxSelect from 'components/box-select/use-box-select';
import { Layout, LayoutComponent, LayoutProps } from './types';
import { Global } from '@emotion/react';

const Container = styled.div({
  height: '100%',
  width: '100%',
  overflow: 'auto',
  scrollbarWidth: 'none',
  '&::-webkit-scrollbar': {
    display: 'none',
  },
});

const LayoutItem = styled.div({
  '& > *': {
    pointerEvents: 'initial',
  },
});

const LayoutScene = styled.div({
  pointerEvents: 'none',
  position: 'relative',
  userSelect: 'none',
  overflow: 'hidden',
});

interface LayoutContainerLayoutState<TLayoutData, TData> {
  layoutData: TLayoutData;
  layoutProps: LayoutProps;
  data: TData;
}

export interface LayoutContainerProps<TData extends ReadonlyArray<unknown>, TLayoutHint> {
  data: TData;
  layout: Layout<TData, TLayoutHint>;
  className?: string;
  isSelected?: (item: TData[number]) => boolean;
  onSelectStart?: (e: GeneralMouseEvent) => void;
  onSelectEnd?: (item: ReadonlyArray<TData[number]>) => void;
  onMouseDownItem?: (item: TData[number], e: GeneralMouseEvent) => void;
}

export function LayoutContainer<TData extends ReadonlyArray<unknown>, TLayoutHint>({
  data,
  layout,
  className,
  isSelected,
  onSelectStart,
  onSelectEnd,
  onMouseDownItem,
}: LayoutContainerProps<TData, TLayoutHint>) {
  const rootRef = useRef<HTMLDivElement>(null);
  const { width, height, clientRect } = useElementSize(rootRef);
  const containerSize = useMemo(() => width && height && { width, height }, [width, height]);

  const [layoutState, setLayoutState] =
    useState<LayoutContainerLayoutState<Exclude<ReturnType<typeof layout.manager.layout>, false>, TData>>();
  const [scrollPosition, setScrollPosition] = useState<{ scrollTop: number; scrollLeft: number }>({ scrollTop: 0, scrollLeft: 0 });

  const UpdateLayout = useCallback(
    (
      containerSize: ISize,
      scrollPosition: { scrollTop: number; scrollLeft: number },
      data: TData,
      prevLayoutState?: LayoutContainerLayoutState<Exclude<ReturnType<typeof layout.manager.layout>, false>, TData>,
    ) => {
      const { layoutData: prevLayoutData, layoutProps: prevLayoutProps } = prevLayoutState ?? {};

      const totalCount = data.length;
      const windowRect = {
        top: scrollPosition.scrollTop,
        right: scrollPosition.scrollLeft + containerSize.width,
        bottom: scrollPosition.scrollTop + containerSize.height,
        left: scrollPosition.scrollLeft,
      };
      const layoutProps: LayoutProps = { totalCount, containerSize, windowRect };

      let layoutData = layout.manager.layout({ layoutProps, prevLayoutData, prevLayoutProps });

      if (layoutData === false) {
        if (!prevLayoutData) {
          throw new Error();
        }
        layoutData = prevLayoutData;
      }

      return {
        windowRect,
        layoutProps,
        layoutData,
        data,
      };
    },
    [layout],
  );

  const handleScroll = useCallback((e: React.UIEvent<HTMLDivElement>) => {
    setScrollPosition({ scrollTop: e.currentTarget.scrollTop, scrollLeft: e.currentTarget.scrollLeft });
  }, []);

  const handleSelectStart = useCallback(
    (e: MouseEvent) => {
      if (layoutState) {
        typeof onSelectStart === 'function' && onSelectStart(e);
      }
    },
    [layoutState, onSelectStart],
  );

  const handleSelectEnd = useCallback(
    (selectingRect: IRect) => {
      if (layoutState) {
        const selectResult =
          rectSize(selectingRect) > 0
            ? layout.manager.select({
                selectProps: { selectRect: selectingRect },
                layoutProps: layoutState.layoutProps,
                layoutData: layoutState.layoutData,
              })
            : { items: [] };
        typeof onSelectEnd === 'function' && onSelectEnd(selectResult.items.map((item) => data[item]));
      }
    },
    [data, layout.manager, layoutState, onSelectEnd],
  );

  const { selectingRect } = useBoxSelect(rootRef, clientRect, { onSelectStart: handleSelectStart, onSelectEnd: handleSelectEnd });

  const handleMouseDownItem = useCallback(
    (data: TData[number], e: GeneralMouseEvent) => {
      typeof onMouseDownItem === 'function' && onMouseDownItem(data, e);
    },
    [onMouseDownItem],
  );

  // recalculate layout
  useEffect(() => {
    if (containerSize) {
      setLayoutState((prev) => {
        return UpdateLayout(containerSize, scrollPosition, data, prev);
      });
    }
  }, [containerSize, UpdateLayout, scrollPosition, data, selectingRect]);

  const content = useMemo(
    () =>
      layoutState?.layoutData.items.map((item) => {
        const itemData = layoutState.data[item.index];
        const selecting = selectingRect ? rectHasIntersection(selectingRect, item.position) : false;
        const selected = typeof isSelected === 'function' ? isSelected(itemData) : false;
        return (
          <LayoutItemContainer
            key={item.index}
            component={layout.component}
            data={itemData}
            position={item.position}
            selected={selecting || selected}
            onMouseDown={handleMouseDownItem}
          />
        );
      }),
    [layoutState?.layoutData.items, layoutState?.data, selectingRect, isSelected, layout.component, handleMouseDownItem],
  );

  return (
    <Container className={className} onScroll={handleScroll} ref={rootRef}>
      {layoutState && (
        <LayoutScene
          style={{
            height: Math.max(layoutState.layoutData.sceneSize.height, height ?? 0),
            width: Math.max(layoutState.layoutData.sceneSize.width, width ?? 0),
          }}
        >
          {content}
          {selectingRect && (
            <>
              <div
                style={{
                  position: 'absolute',
                  background: 'rgba(0, 0, 0, 0.1)',
                  border: '1px solid rgba(0, 0, 0, 0.32)',
                  left: selectingRect.left,
                  top: selectingRect.top,
                  height: selectingRect.bottom - selectingRect.top,
                  width: selectingRect.right - selectingRect.left,
                  willChange: 'width, height, left, top',
                }}
              />
              <Global
                styles={{
                  '*': {
                    userSelect: 'none !important' as 'none',
                    pointerEvents: 'none !important' as 'none',
                  },
                }}
              />
            </>
          )}
        </LayoutScene>
      )}
    </Container>
  );
}

interface LayoutItemContainerProps<TData> {
  position: IRect;
  data: TData;
  component: LayoutComponent<TData>;
  selected: boolean;
  onMouseDown?: (data: TData, e: GeneralMouseEvent) => void;
}

const LayoutItemContainer = memo(
  <TData,>({ position, data, component: LayoutComponent, selected, onMouseDown }: LayoutItemContainerProps<TData>) => {
    const width = position.right - position.left;
    const height = position.bottom - position.top;

    const positionStyle = useMemo(
      () => ({
        position: 'absolute' as const,
        left: position.left,
        top: position.top,
        width,
        height,
      }),
      [height, position.left, position.top, width],
    );
    const viewport = useMemo(() => ({ width, height }), [height, width]);

    const handleMouseDown = useCallback(
      (e: GeneralMouseEvent) => {
        typeof onMouseDown === 'function' && onMouseDown(data, e);
      },
      [data, onMouseDown],
    );

    return (
      <LayoutItem style={positionStyle} onMouseDown={handleMouseDown}>
        <LayoutComponent data={data} viewport={viewport} selecting={selected} />
      </LayoutItem>
    );
  },
);

LayoutItemContainer.displayName = 'LayoutItemContainer';
