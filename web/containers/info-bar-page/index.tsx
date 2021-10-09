import { useFileInfoByFileHandleQuery } from 'api';
import InfoBarLayout from 'components/layout/info-bar-layout';
import SingleFileInfo from 'components/single-file-info';
import { useSelection } from 'containers/selection';
import InfoBarHeader from './header';

const InfoBarPage: React.FC = () => {
  const { selected } = useSelection();

  return (
    <InfoBarLayout>
      <InfoBarHeader />
      {selected.size === 1 && <SingleFileInfoBarPage fileHandle={{ identifier: selected.values().next().value }} />}
    </InfoBarLayout>
  );
};

const SingleFileInfoBarPage: React.FC<{ fileHandle: FileHandle }> = ({ fileHandle }) => {
  const { data } = useFileInfoByFileHandleQuery({
    variables: {
      fileHandle,
    },
  });

  return data ? <SingleFileInfo file={data.openFileHandle.openFile} /> : <></>;
};

export default InfoBarPage;
