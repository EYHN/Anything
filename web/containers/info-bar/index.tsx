import styled from '@emotion/styled';
import { useFileInfoByFileHandleQuery, useSetTagsMutation, useSetNotesMutation } from 'api';
import SingleFileInfo from 'components/single-file-info';
import { useSelection } from 'containers/selection';
import { useCallback } from 'react';

export { default as InfoBarHeader } from './header';

const InfoBarContainer = styled.div(({ theme }) => ({
  padding: '16px',
  color: theme.colors.gray100,
  height: '100%',
  overflow: 'auto',
  scrollbarWidth: 'none',
}));

export const InfoBar: React.VoidFunctionComponent = () => {
  const { selected } = useSelection();

  return (
    <InfoBarContainer>
      {selected.size === 1 && <SingleFileInfoBarPage fileHandle={{ identifier: selected.values().next().value }} />}
    </InfoBarContainer>
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
