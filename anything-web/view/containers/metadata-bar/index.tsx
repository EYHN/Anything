import { useSelection } from 'containers/selection';
import React from 'react';
import SingleFileMetadataBar from './single-file';

const MetadataBar: React.FunctionComponent = () => {
  const [selection] = useSelection();

  if (selection.length == 1) {
    return <SingleFileMetadataBar url={selection[0]} />;
  }

  return <></>;
};

export default React.memo(MetadataBar);
