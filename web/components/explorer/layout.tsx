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
  viewport: ISize;
}

export interface LayoutData {
  items: LayoutItem[];
  totalSize: ISize;
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
});

export function LayoutContainer<TItemData, TData extends ReadonlyArray<TItemData>>({
  data,
  layout,
  className,
}: LayoutContainerProps<TItemData, TData>) {
  const LayoutView = layout.view;

  const rootRef = useRef<HTMLDivElement>(null);
  const { width, height } = useElementSize(rootRef);
  const viewport = useMemo(() => width && height && { width, height }, [width, height]);

  const [renderData, setRenderData] =
    useState<{ layoutData: ReturnType<typeof layout.manager.layout>; layoutProps: LayoutProps; windowRect: IRect; data: TData }>();
  const [scrollPosition, setScrollPosition] = useState<{ scrollTop: number; scrollLeft: number }>({ scrollTop: 0, scrollLeft: 0 });

  const UpdateLayout = useCallback(
    (
      viewport: ISize,
      scrollPosition: { scrollTop: number; scrollLeft: number },
      totalCount: number,
      prevLayoutState?: { layoutData?: ReturnType<typeof layout.manager.layout>; layoutProps?: LayoutProps; windowRect?: IRect },
    ) => {
      const layoutProps = { totalCount, viewport };
      const windowRect = {
        top: scrollPosition.scrollTop,
        right: scrollPosition.scrollLeft + viewport.width,
        bottom: scrollPosition.scrollTop + viewport.height,
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
        return {
          windowRect: prevWindowRect,
          layoutData: prevLayoutData,
          layoutProps: prevLayoutProps,
        };
      }

      return {
        windowRect: windowRect,
        layoutProps: layoutProps,
        layoutData: layout.manager.layout({ layoutProps, windowRect }),
      };
    },
    [layout],
  );

  const handleScroll = useCallback((e: UIEvent<HTMLDivElement>) => {
    setScrollPosition({ scrollTop: e.currentTarget.scrollTop, scrollLeft: e.currentTarget.scrollLeft });
  }, []);

  // recalculate layout
  useEffect(() => {
    if (viewport) {
      setRenderData((prev) => {
        const { layoutData, layoutProps, windowRect } = UpdateLayout(viewport, scrollPosition, data.length, prev);
        return {
          layoutData,
          layoutProps,
          windowRect,
          data,
        };
      });
    }
  }, [viewport, UpdateLayout, scrollPosition, data]);

  const content = useMemo(
    () =>
      renderData && (
        <div
          style={{
            height: renderData.layoutData.totalSize.height,
            width: renderData.layoutData.totalSize.width,
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
