import { useSelection } from 'containers/Selection';
import React from 'react';
import SingleFileMetadataBar from './SingleFile';

interface MetadataBar {
}

const MetadataBar: React.FunctionComponent<MetadataBar> = ({}) => {
  const [selection] = useSelection();

  if (selection.length == 1) {
    return <SingleFileMetadataBar path={selection[0]} />
  }

  return <></>
}

export default React.memo(MetadataBar);