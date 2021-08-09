import styled from '@emotion/styled';

import { UIEvent, useCallback, useEffect, useMemo, useRef, useState } from 'react';
import useElementSize from 'components/use-element-size';

import { IRect } from 'utils/rect';
import { ISize } from 'utils/size';
import useBoxSelect from 'components/box-select/use-box-select';

export interface LayoutItem {
  index: number;
  position: IRect;
}

export interface LayoutProps {
  totalCount: number;
  containerSize: ISize;
}

export interface LayoutData {
  items: LayoutItem[];
  sceneSize: ISize;
}

export interface LayoutManager<TLayoutHintData = unknown> {
  layout: (options: { layoutProps: LayoutProps; windowRect: IRect }) => LayoutData & { hint: TLayoutHintData };
  select: (options: { layoutProps: LayoutProps; layoutData: LayoutData & { hint: TLayoutHintData }; selectRect: IRect }) => {
    items: { index: number }[];
  };

  shouldUpdateLayout?: (options: {
    newLayoutProps: LayoutProps;
    newWindowRect: IRect;
    prevLayoutProps: LayoutProps;
    prevWindowRect: IRect;
    prevLayoutData: LayoutData & { hint: TLayoutHintData };
  }) => boolean;
}

export interface LayoutViewProps<TData = unknown> {
  data: TData;
  viewport: ISize;
  selected: boolean;
}

export type LayoutView<TData = unknown> = React.ComponentType<LayoutViewProps<TData>>;

export interface Layout<TData> {
  manager: LayoutManager;
  view: LayoutView<TData>;
}

export interface LayoutContainerProps<TItemData, TData extends ReadonlyArray<TItemData>> {
  data: TData;
  layout: Layout<TItemData>;
  className?: string;
}

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

export function LayoutContainer<TItemData, TData extends ReadonlyArray<TItemData>>({
  data,
  layout,
  className,
}: LayoutContainerProps<TItemData, TData>) {
  const LayoutView = layout.view;

  const rootRef = useRef<HTMLDivElement>(null);
  const { width, height, clientRect } = useElementSize(rootRef);
  const containerSize = useMemo(() => width && height && { width, height }, [width, height]);

  const [renderData, setRenderData] =
    useState<{ layoutData: ReturnType<typeof layout.manager.layout>; layoutProps: LayoutProps; windowRect: IRect; data: TData }>();
  const [scrollPosition, setScrollPosition] = useState<{ scrollTop: number; scrollLeft: number }>({ scrollTop: 0, scrollLeft: 0 });

  const UpdateLayout = useCallback(
    (
      containerSize: ISize,
      scrollPosition: { scrollTop: number; scrollLeft: number },
      data: TData,
      prevLayoutState?: {
        layoutData: ReturnType<typeof layout.manager.layout>;
        layoutProps: LayoutProps;
        windowRect: IRect;
        data: TData;
      },
    ) => {
      const totalCount = data.length;
      const layoutProps = { totalCount, containerSize };
      const windowRect = {
        top: scrollPosition.scrollTop,
        right: scrollPosition.scrollLeft + containerSize.width,
        bottom: scrollPosition.scrollTop + containerSize.height,
        left: scrollPosition.scrollLeft,
      };

      const { layoutData: prevLayoutData, layoutProps: prevLayoutProps, windowRect: prevWindowRect } = prevLayoutState ?? {};

      if (
        prevLayoutData &&
        prevLayoutProps &&
        prevWindowRect &&
        layout.manager.shouldUpdateLayout &&
        !layout.manager.shouldUpdateLayout({
          prevWindowRect,
          prevLayoutProps,
          prevLayoutData,
          newWindowRect: windowRect,
          newLayoutProps: layoutProps,
        })
      ) {
        return prevLayoutState as Required<NonNullable<typeof prevLayoutState>>;
      }

      return {
        windowRect: windowRect,
        layoutProps: layoutProps,
        layoutData: layout.manager.layout({ layoutProps, windowRect }),
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
      setRenderData((prev) => {
        return UpdateLayout(containerSize, scrollPosition, data, prev);
      });
    }
  }, [containerSize, UpdateLayout, scrollPosition, data]);

  const content = useMemo(
    () =>
      renderData &&
      renderData.layoutData.items.map((item) => {
        const itemData = renderData.data[item.index];
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
            <LayoutView
              data={itemData}
              viewport={{ width: item.position.right - item.position.left, height: item.position.bottom - item.position.top }}
              selected={false}
            />
          </LayoutViewContainer>
        );
      }),
    [LayoutView, renderData],
  );

  const { selectionRect } = useBoxSelect(rootRef, clientRect);

  return (
    <Container className={className} onScroll={handleScroll} ref={rootRef} selecting={!!selectionRect}>
      {renderData && (
        <LayoutScene
          style={{
            height: Math.max(renderData.layoutData.sceneSize.height, height ?? 0),
            width: Math.max(renderData.layoutData.sceneSize.width, width ?? 0),
          }}
        >
          {content}
          {selectionRect && (
            <div
              style={{
                position: 'absolute',
                background: 'rgba(0, 0, 0, 0.1)',
                border: '1px solid rgba(0, 0, 0, 0.32)',
                left: selectionRect.left,
                top: selectionRect.top,
                height: selectionRect.bottom - selectionRect.top,
                width: selectionRect.right - selectionRect.left,
                willChange: 'width, height, left, top',
              }}
            />
          )}
        </LayoutScene>
      )}
    </Container>
  );
}
