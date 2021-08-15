import styled from '@emotion/styled';

import { UIEvent, useCallback, useEffect, useMemo, useRef, useState } from 'react';
import useElementSize from 'components/use-element-size';

import { IRect } from 'utils/rect';
import { ISize } from 'utils/size';
import useBoxSelect from 'components/box-select/use-box-select';
import { Layout, LayoutProps } from './types';

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

const LayoutViewContainer = styled.div({
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
}

export function LayoutContainer<TData extends ReadonlyArray<unknown>, TLayoutHint>({
  data,
  layout,
  className,
}: LayoutContainerProps<TData, TLayoutHint>) {
  const LayoutComponent = layout.component;

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
      selectingRect: IRect | undefined,
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
      const layoutProps: LayoutProps = { totalCount, containerSize, windowRect, selectingRect };

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
        return UpdateLayout(containerSize, scrollPosition, selectingRect, data, prev);
      });
    }
  }, [containerSize, UpdateLayout, scrollPosition, data, selectingRect]);

  const content = useMemo(
    () =>
      layoutState?.layoutData.items.map((item) => {
        const itemData = layoutState.data[item.index];
        return (
          <LayoutViewContainer
            key={item.index}
            style={{
              position: 'absolute',
              left: item.position.left,
              top: item.position.top,
              width: item.position.right - item.position.left,
              height: item.position.bottom - item.position.top,
            }}
          >
            <LayoutComponent
              data={itemData}
              viewport={{ width: item.position.right - item.position.left, height: item.position.bottom - item.position.top }}
              selecting={!!item.selecting}
            />
          </LayoutViewContainer>
        );
      }),
    [LayoutComponent, layoutState?.layoutData, layoutState?.data],
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
