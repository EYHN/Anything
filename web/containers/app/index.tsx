import React, { useState } from 'react';
import Helmet from './helmet';

import { Route } from 'react-router-dom';
import { useListFilesQuery } from 'api';
import NavBar from 'components/nav-bar';
import AppLayout from 'components/layout/app-layout';
import InfoBar from 'components/info-bar';
import ToolBar from 'components/tool-bar';

const Loader: React.FC = () => {
  const [activeUrl] = useState<string>('file://local/');
  // const [size, setSize] = React.useState(600);

  const { data } = useListFilesQuery({
    variables: {
      url: activeUrl,
    },
    fetchPolicy: 'cache-and-network',
  });

  // const handleOnOpen = React.useCallback((file: IFileFragment) => {
  //   setActiveUrl(file.url);
  // }, []);

  // if (error) return <p>Error :(</p>;

  const left = <NavBar />;

  const right = <InfoBar />;

  // const bottom = () => (
  //   <>
  //     <input type="range" id="volume" name="volume" min="0" max="700" value={size} onChange={(e) => setSize(parseInt(e.target.value))} />
  //     <label htmlFor="volume">大小</label>
  //   </>
  // );

  // const center = (width: number, height: number) => (
  //   <GridLayout key={activeUrl} files={data?.directory.entries || []} size={size} onOpen={handleOnOpen} viewport={{ width, height }} />
  // );

  const center = (
    <>
      <pre>{JSON.stringify(data, null, 2)}</pre>
    </>
  );

  return <AppLayout tooltip={<ToolBar />} left={left} right={right} center={center} />;
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
