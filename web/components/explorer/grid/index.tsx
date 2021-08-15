import { memo } from 'react';
import { IFileFragment } from 'api';
import GridLayoutManager, { GridLayoutManagerHint } from './layout-manager';
import File from './file';
import { Layout, LayoutComponentProps } from '../types';

const GridLayoutComponent: React.FC<LayoutComponentProps<IFileFragment>> = memo(({ data, viewport, selecting }) => (
  <File selecting={selecting} width={viewport.width} height={viewport.height} file={data} />
));

GridLayoutComponent.displayName = 'memo(GridLayoutView)';

export const GridLayout: Layout<ReadonlyArray<IFileFragment>, GridLayoutManagerHint> = {
  manager: GridLayoutManager,
  component: GridLayoutComponent,
};
