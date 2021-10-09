import { memo } from 'react';
import { IDirentFragment } from 'api';
import GridLayoutManager, { GridLayoutManagerHint } from './layout-manager';
import File from './file';
import { Layout, LayoutComponentProps } from '../types';

const GridLayoutComponent: React.FC<LayoutComponentProps<IDirentFragment>> = memo(({ data, viewport, selecting }) => (
  <File selecting={selecting} width={viewport.width} height={viewport.height} dirent={data} />
));

GridLayoutComponent.displayName = 'memo(GridLayoutView)';

export const GridLayout: Layout<ReadonlyArray<IDirentFragment>, GridLayoutManagerHint> = {
  manager: GridLayoutManager,
  component: GridLayoutComponent,
};
