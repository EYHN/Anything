import { memo } from 'react';
import { IFileFragment } from 'api';
import { Layout, LayoutViewProps } from '../layout';
import GridLayoutManager from './layout-manager';
import File from './file';

const GridLayoutView: React.FC<LayoutViewProps<IFileFragment>> = memo(({ data, viewport }) => (
  <File focus={false} width={viewport.width} height={viewport.height} file={data} />
));

GridLayoutView.displayName = 'memo(GridLayoutView)';

export const GridLayout: Layout<IFileFragment> = {
  manager: GridLayoutManager,
  view: GridLayoutView,
};
