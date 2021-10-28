import { useAddTagsMutation, useFileInfoByFileHandleQuery, useRemoveTagsMutation, useSetNoteMutation } from 'api';
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
  const [setNoteMutation] = useSetNoteMutation();

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

  const handleChangeNote = useCallback(
    (newNote: string) => {
      if (!file) return;
      setNoteMutation({
        variables: { fileHandle: file.fileHandle.value, note: newNote },
        optimisticResponse: { setNote: { __typename: file.__typename, _id: file._id, note: newNote } },
      });
    },
    [file, setNoteMutation],
  );

  return file ? (
    <SingleFileInfo
      file={file}
      key={fileHandle.identifier}
      onAddTag={handleAddTag}
      onRemoveTag={handleRemoveTag}
      onChangeNote={handleChangeNote}
    />
  ) : (
    <></>
  );
};

export default InfoBarPage;
