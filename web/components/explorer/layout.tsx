import styled from '@emotion/styled';

import { memo, UIEvent, useCallback, useEffect, useMemo, useRef, useState } from 'react';
import useElementSize from 'components/use-element-size';

import { IRect, rectHasIntersection } from 'utils/rect';
import { ISize } from 'utils/size';
import useBoxSelect from 'components/box-select/use-box-select';
import { Layout, LayoutComponent, LayoutProps } from './types';

const Container = styled.div<{ selecting: boolean }>(({ selecting }) => ({
  height: '100%',
  width: '100%',
  overflow: 'auto',
  scrollbarWidth: 'none',
  '&::-webkit-scrollbar': {
    display: 'none',
  },
  '& *': {
    pointerEvents: selecting ? ('none !important' as 'none') : undefined,
    userSelect: selecting ? ('none !important' as 'none') : undefined,
  },
}));

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
}

export function LayoutContainer<TData extends ReadonlyArray<unknown>, TLayoutHint>({
  data,
  layout,
  className,
  isSelected,
}: LayoutContainerProps<TData, TLayoutHint>) {
  const rootRef = useRef<HTMLDivElement>(null);
  const { width, height, clientRect } = useElementSize(rootRef);
  const containerSize = useMemo(() => width && height && { width, height }, [width, height]);
  const { selectingRect } = useBoxSelect(rootRef, clientRect);

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

  const handleScroll = useCallback((e: UIEvent<HTMLDivElement>) => {
    setScrollPosition({ scrollTop: e.currentTarget.scrollTop, scrollLeft: e.currentTarget.scrollLeft });
  }, []);

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
          />
        );
      }),
    [layoutState?.layoutData.items, layoutState?.data, selectingRect, isSelected, layout.component],
  );

  return (
    <Container className={className} onScroll={handleScroll} ref={rootRef} selecting={!!selectingRect}>
      {layoutState && (
        <LayoutScene
          style={{
            height: Math.max(layoutState.layoutData.sceneSize.height, height ?? 0),
            width: Math.max(layoutState.layoutData.sceneSize.width, width ?? 0),
          }}
        >
          {content}
          {selectingRect && (
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
}

const LayoutItemContainer = memo(<TData,>({ position, data, component: LayoutComponent, selected }: LayoutItemContainerProps<TData>) => {
  return (
    <LayoutItem
      style={{
        position: 'absolute',
        left: position.left,
        top: position.top,
        width: position.right - position.left,
        height: position.bottom - position.top,
      }}
    >
      <LayoutComponent
        data={data}
        viewport={{ width: position.right - position.left, height: position.bottom - position.top }}
        selecting={selected}
      />
    </LayoutItem>
  );
});

LayoutItemContainer.displayName = 'LayoutItemContainer';
