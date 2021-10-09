import React, { useState } from 'react';
import Head from './helmet';

import { Route } from 'react-router-dom';
import { useListFilesByUrlQuery } from 'api';
import NavBar from 'components/nav-bar';
import AppLayout from 'components/layout/app-layout';
import ToolBar from 'components/tool-bar';
import Explorer from 'components/explorer';
import InfoBarPage from 'containers/info-bar-page';

const Loader: React.FC = () => {
  const [activeUrl] = useState<string>('file://local/');
  // const [size, setSize] = React.useState(600);

  const { data } = useListFilesByUrlQuery({
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

  const right = <InfoBarPage />;

  // const bottom = () => (
  //   <>
  //     <input type="range" id="volume" name="volume" min="0" max="700" value={size} onChange={(e) => setSize(parseInt(e.target.value))} />
  //     <label htmlFor="volume">大小</label>
  //   </>
  // );

  // const center = (width: number, height: number) => (
  //   <GridLayout key={activeUrl} files={data?.directory.entries || []} size={size} onOpen={handleOnOpen} viewport={{ width, height }} />
  // );

  const explorer = <Explorer files={data?.createFileHandle.openDirectory.entries ?? []} />;

  return <AppLayout tooltip={<ToolBar />} left={left} right={right} explorer={explorer} />;
};

const App: React.FunctionComponent = () => {
  return (
    <>
      <Head />
      <Route path="/" component={Loader} />
    </>
  );
};

export default App;
