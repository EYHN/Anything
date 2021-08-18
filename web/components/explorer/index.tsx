import { IFileFragment } from 'api';
import { GridLayout } from './grid';
import { LayoutContainer } from './layout';

import { useSelection } from 'containers/selection';
import { useCallback } from 'react';

interface Props {
  className?: string;
  files: ReadonlyArray<IFileFragment>;
}

const Explorer: React.FC<Props> = ({ className, files }) => {
  const { selected } = useSelection();

  const isSelected = useCallback(
    (file: IFileFragment) => {
      return selected.has(file.url);
    },
    [selected],
  );

  return <LayoutContainer className={className} data={files} layout={GridLayout} isSelected={isSelected} />;
};

export default Explorer;
