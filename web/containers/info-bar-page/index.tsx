import { useAddTagsMutation, useFileInfoByFileHandleQuery, useRemoveTagsMutation, useSetNotesMutation } from 'api';
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

  const [addTagsMutation] = useAddTagsMutation();
  const [removeTagsMutation] = useRemoveTagsMutation();
  const [setNotesMutation] = useSetNotesMutation();

  const handleAddTag = useCallback(
    (tag: string) => {
      if (!file) return;
      addTagsMutation({
        variables: { fileHandle: file.fileHandle.value, tags: [tag] },
        optimisticResponse: { addTags: { __typename: file.__typename, _id: file._id, tags: [...file.tags, tag] } },
      });
    },
    [addTagsMutation, file],
  );

  const handleRemoveTag = useCallback(
    (tag: string) => {
      if (!file) return;
      removeTagsMutation({
        variables: { fileHandle: file.fileHandle.value, tags: [tag] },
        optimisticResponse: { removeTags: { __typename: file.__typename, _id: file._id, tags: file.tags.filter((t) => t !== tag) } },
      });
    },
    [file, removeTagsMutation],
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

  return file ? (
    <SingleFileInfo
      file={file}
      key={fileHandle.identifier}
      onAddTag={handleAddTag}
      onRemoveTag={handleRemoveTag}
      onSetNotes={handleSetNotes}
    />
  ) : (
    <></>
  );
};

export default InfoBarPage;
