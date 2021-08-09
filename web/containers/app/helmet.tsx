import Helmet from 'components/helmet';

//@ts-expect-error
import sanitizeCSS from 'sanitize.css/sanitize.css';

//@ts-expect-error
import globalCSS from '../../global.css';

const Head: React.FunctionComponent = () => (
  <>
    <Helmet>
      <meta charSet="UTF-8" />
      <meta httpEquiv="X-UA-Compatible" content="IE=edge" />
      <meta name="viewport" content="width=device-width, initial-scale=1" />
      <link href="https://fonts.googleapis.com/css2?family=Inter&display=swap" rel="stylesheet"></link>
    </Helmet>
    <style type="text/css">{sanitizeCSS}</style>
    <style type="text/css">{globalCSS}</style>
  </>
);

export default Head;
