import { useFileInfoByFileHandleQuery, useSetTagsMutation, useSetNotesMutation } from 'api';
import InfoBarLayout from 'components/layout/info-bar-layout';
import SingleFileInfo from 'components/single-file-info';
import { useSelection } from 'containers/selection';
import { useCallback } from 'react';
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

  const file = data?.openFileHandle.openFile;

  const [setTagsMutation] = useSetTagsMutation();
  const [setNotesMutation] = useSetNotesMutation();

  const handleSetTags = useCallback(
    (tags: string[]) => {
      if (!file) return;
      setTagsMutation({
        variables: { fileHandle: file.fileHandle.value, tags: tags },
        optimisticResponse: { setTags: { __typename: file.__typename, _id: file._id, tags: tags } },
      });
    },
    [file, setTagsMutation],
  );

  const handleSetNotes = useCallback(
    (newNotes: string) => {
      if (!file) return;
      setNotesMutation({
        variables: { fileHandle: file.fileHandle.value, notes: newNotes },
        optimisticResponse: { setNotes: { __typename: file.__typename, _id: file._id, notes: newNotes } },
      });
    },
    [file, setNotesMutation],
  );

  return file ? <SingleFileInfo file={file} key={fileHandle.identifier} onSetTags={handleSetTags} onSetNotes={handleSetNotes} /> : <></>;
};

export default InfoBarPage;
