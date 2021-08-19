import { IRect } from 'utils/rect';
import { ISize } from 'utils/size';

export interface LayoutProps {
  totalCount: number;
  containerSize: ISize;
  windowRect: IRect;
}

export interface LayoutItem {
  index: number;
  position: IRect;
}

export interface LayoutData {
  items: LayoutItem[];
  sceneSize: ISize;
}

export interface SelectProps {
  selectRect: IRect;
}

export interface SelectData {
  items: number[];
}

export interface LayoutManager<TLayoutHint = unknown> {
  layout: (options: {
    layoutProps: LayoutProps;
    prevLayoutProps?: LayoutProps;
    prevLayoutData?: LayoutData & { hint: TLayoutHint };
  }) => (LayoutData & { hint: TLayoutHint }) | false;
  select: (options: { selectProps: SelectProps; layoutProps: LayoutProps; layoutData: LayoutData & { hint: TLayoutHint } }) => SelectData;
}

export interface LayoutComponentProps<TData> {
  data: TData;
  viewport: ISize;
  selecting: boolean;
}

export type LayoutComponent<TData = unknown> = React.ComponentType<LayoutComponentProps<TData>>;

export interface Layout<TData extends ReadonlyArray<unknown>, TLayoutHint> {
  manager: LayoutManager<TLayoutHint>;
  component: LayoutComponent<TData[number]>;
}
