import React, { useState } from 'react';
import Head from './helmet';

import { Route } from 'react-router-dom';
import { useListFilesByUrlQuery } from 'api';
import NavBar from 'components/nav-bar';
import AppLayout from 'containers/app/layout';
import ToolBar from 'components/tool-bar';
import Explorer from 'components/explorer';
import { InfoBar, InfoBarHeader } from 'containers/info-bar';

const Loader: React.FC = () => {
  const [activeUrl] = useState<string>('file://local/');

  const { data } = useListFilesByUrlQuery({
    variables: {
      url: activeUrl,
    },
    fetchPolicy: 'cache-and-network',
  });

  const left = <NavBar />;

  return (
    <AppLayout
      tooltip={<ToolBar />}
      left={left}
      infoBarHeader={<InfoBarHeader />}
      infoBar={<InfoBar />}
      explorer={<Explorer files={data?.createFileHandle.openDirectory.entries ?? []} />}
    />
  );
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
