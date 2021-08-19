import { useFileInfoQuery } from 'api';
import InfoBarLayout from 'components/layout/info-bar-layout';
import SingleFileInfo from 'components/single-file-info';
import { useSelection } from 'containers/selection';
import InfoBarHeader from './header';

const InfoBarPage: React.FC = () => {
  const { selected } = useSelection();

  return (
    <InfoBarLayout>
      <InfoBarHeader />
      {selected.size === 1 && <SingleFileInfoBarPage url={selected.values().next().value} />}
    </InfoBarLayout>
  );
};

const SingleFileInfoBarPage: React.FC<{ url: string }> = ({ url }) => {
  const { data } = useFileInfoQuery({
    variables: {
      url,
    },
  });

  return data ? <SingleFileInfo file={data.file} /> : <></>;
};

export default InfoBarPage;
