import styled from '@emotion/styled';

import { UIEvent, useCallback, useEffect, useMemo, useRef, useState } from 'react';
import useElementSize from 'components/use-element-size';

import { IRect } from 'utils/rect';
import { ISize } from 'utils/size';

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

const Container = styled.div({
  height: '100%',
  width: '100%',
  overflow: 'auto',
  scrollbarWidth: 'none',
  '&::-webkit-scrollbar': {
    display: 'none',
  },
});

export function LayoutContainer<TItemData, TData extends ReadonlyArray<TItemData>>({
  data,
  layout,
  className,
}: LayoutContainerProps<TItemData, TData>) {
  const LayoutView = layout.view;

  const rootRef = useRef<HTMLDivElement>(null);
  const { width, height } = useElementSize(rootRef);
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
      renderData && (
        <div
          style={{
            height: renderData.layoutData.sceneSize.height,
            width: renderData.layoutData.sceneSize.width,
            position: 'relative',
            userSelect: 'none',
          }}
        >
          {renderData.layoutData.items.map((item) => {
            const itemData = renderData.data[item.index];
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
                <LayoutView
                  data={itemData}
                  viewport={{ width: item.position.right - item.position.left, height: item.position.bottom - item.position.top }}
                  selected={false}
                />
              </div>
            );
          })}
        </div>
      ),
    [LayoutView, renderData],
  );

  return (
    <Container className={className} onScroll={handleScroll} ref={rootRef}>
      {content}
    </Container>
  );
}
