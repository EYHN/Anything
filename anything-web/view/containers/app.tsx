import React from 'react';
import AppLayout from './layout';
import Helmet from './Helmet';

import { Route, RouteComponentProps } from 'react-router-dom';
import { useListDirectoryQuery } from 'api';
import GridLayout from 'components/Layout/GridLayout';
import ToolBar from 'components/ToolBar';
import SideBar from 'components/SideBar';
import MetadataBar from 'containers/MetadataBar';

const Loader: React.FunctionComponent<RouteComponentProps> = ({ location, history }) => {
  const pathname = location.pathname;
  const [size, setSize] = React.useState(600);

  const { error, data } = useListDirectoryQuery({
    variables: {
      path: pathname,
    },
    fetchPolicy: 'cache-and-network',
  });

  const handleOnOpen = React.useCallback(
    (path: string) => {
      console.log(path);
      history.push(path);
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
    <GridLayout key={pathname} directory={data?.directory} size={size} onOpen={handleOnOpen} width={width} height={height} />
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
