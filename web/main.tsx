import ReactDOM from 'react-dom';
import App from './containers/app';
import { ApolloClient, InMemoryCache, ApolloProvider } from '@apollo/client';

import { BrowserRouter } from 'react-router-dom';
import { Provider as SelectionProvider } from 'containers/selection';
import api from 'api';
import { I18nProvider } from '@lingui/react';
import i18n from './i18n';
import { LightTheme } from '@anything/shared';
import { ThemeProvider } from '@emotion/react';
import { StrictMode } from 'react';

const MOUNT_NODE = document.body;

const root = document.createElement('div');
root.id = 'app';
MOUNT_NODE.appendChild(root);

const client = new ApolloClient({
  uri: '/api/graphql',
  cache: new InMemoryCache({ possibleTypes: api.possibleTypes, typePolicies: {} }),
});

const render = (Content: React.ComponentType) => {
  ReactDOM.render(
    <StrictMode>
      <ThemeProvider theme={LightTheme}>
        <I18nProvider i18n={i18n}>
          <ApolloProvider client={client}>
            <BrowserRouter>
              <SelectionProvider>
                <Content />
              </SelectionProvider>
            </BrowserRouter>
          </ApolloProvider>
        </I18nProvider>
      </ThemeProvider>
    </StrictMode>,
    root,
  );
};

render(App);

if (module.hot) {
  module.hot.accept(['./containers/app'], () => {
    ReactDOM.unmountComponentAtNode(MOUNT_NODE);
    // eslint-disable-next-line @typescript-eslint/no-var-requires
    render(require('./containers/app').default);
  });
}
