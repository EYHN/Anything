import { IFileInfoFragment } from 'api';
import Title from './title';
import MetadataEntryGroup from './metadata';
import { useMemo } from 'react';
import { parseMetadataPayload } from 'metadata';
import Tags, { TagsProps } from './tags';
import Note, { NoteProps } from './note';

interface Props {
  file: IFileInfoFragment;
  onAddTag?: TagsProps['onAddTag'];
  onRemoveTag?: TagsProps['onRemoveTag'];
  onChangeNote?: NoteProps['onChange'];
}

const SingleFileInfo: React.FC<Props> = ({ file, onAddTag, onRemoveTag, onChangeNote }) => {
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
      <Note note={file.note} onChange={onChangeNote} />
      {metadataList}
    </>
  );
};

export default SingleFileInfo;
