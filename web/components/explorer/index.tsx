import { IFileFragment } from 'api';
import { GridLayout } from './grid';
import { LayoutContainer } from './layout';

interface Props {
  className?: string;
  files: ReadonlyArray<IFileFragment>;
}

const Explorer: React.FC<Props> = ({ className, files }) => {
  return <LayoutContainer className={className} data={files} layout={GridLayout} />;
};

export default Explorer;
