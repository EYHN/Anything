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

export interface LayoutManager<TLayoutHintData> {
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
