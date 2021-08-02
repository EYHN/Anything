import { memo } from 'react';
import { useSelection } from 'containers/selection';
import SingleFileMetadataBar from './single-file';

const MetadataBar: React.FunctionComponent = () => {
  const [selection] = useSelection();

  if (selection.length == 1) {
    return <SingleFileMetadataBar url={selection[0]} />;
  }

  return <></>;
};

export default memo(MetadataBar);
