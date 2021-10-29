import { IFileInfoFragment } from 'api';
import Title from './title';
import MetadataEntryGroup from './metadata';
import { useMemo } from 'react';
import { parseMetadataPayload } from 'metadata';
import Tags, { TagsProps } from './tags';
import Notes, { NotesProps } from './notes';

interface Props {
  file: IFileInfoFragment;
  onAddTag?: TagsProps['onAddTag'];
  onRemoveTag?: TagsProps['onRemoveTag'];
  onSetNotes?: NotesProps['onChange'];
}

const SingleFileInfo: React.FC<Props> = ({ file, onAddTag, onRemoveTag, onSetNotes }) => {
  const metadataList = useMemo(() => {
    const groupedMetadata = parseMetadataPayload(file.metadata);

    const groupKeys = Reflect.ownKeys(groupedMetadata) as (keyof typeof groupedMetadata)[];

    return groupKeys.map((groupKey) => {
      const items = groupedMetadata[groupKey]!;
      return <MetadataEntryGroup key={groupKey} groupKey={groupKey} entries={items} />;
    });
  }, [file.metadata]);

  return (
    <>
      <Title file={file} />
      <Tags tags={file.tags} onAddTag={onAddTag} onRemoveTag={onRemoveTag} />
      <Notes notes={file.notes} onChange={onSetNotes} />
      {metadataList}
    </>
  );
};

export default SingleFileInfo;
