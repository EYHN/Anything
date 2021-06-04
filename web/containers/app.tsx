import React, { useState } from 'react';
import AppLayout from './layout';
import Helmet from './helmet';

import { Route } from 'react-router-dom';
import { IFileFragment, useListFilesQuery } from 'api';
import GridLayout from 'components/layout/grid-layout';
import ToolBar from 'components/toolbar';
import SideBar from 'components/sidebar';
import MetadataBar from 'containers/metadata-bar';

const Loader: React.FC = () => {
  const [activeUrl, setActiveUrl] = useState<string>('file://local/');
  const [size, setSize] = React.useState(600);

  const { error, data } = useListFilesQuery({
    variables: {
      url: activeUrl,
    },
    fetchPolicy: 'cache-and-network',
  });

  const handleOnOpen = React.useCallback(
    (file: IFileFragment) => {
      setActiveUrl(file.url);
    },
    [history],
  );

  if (error) return <p>Error :(</p>;

  const left = () => <SideBar />;

  const right = () => <MetadataBar />;

  const top = () => (
    <>
      <ToolBar />
    </>
  );

  const bottom = () => (
    <>
      <input type="range" id="volume" name="volume" min="0" max="700" value={size} onChange={(e) => setSize(parseInt(e.target.value))} />
      <label htmlFor="volume">大小</label>
    </>
  );

  const center = (width: number, height: number) => (
    <GridLayout key={activeUrl} files={data?.directory.entries || []} size={size} onOpen={handleOnOpen} width={width} height={height} />
  );

  return <AppLayout top={top} bottom={bottom} center={center} left={left} right={right} />;
};

const App: React.FunctionComponent = () => {
  return (
    <>
      <Helmet />
      <Route path="/" component={Loader} />
    </>
  );
};

export default App;
