import { IFileInfoFragment } from 'api';
import Title from './title';
import MetadataEntryGroup from './metadata';
import { useMemo } from 'react';
import { parseMetadataPayload } from 'metadata';
import TagsGroup, { TagsGroupProps } from './tags';

interface Props {
  file: IFileInfoFragment;
  onAddTag?: TagsGroupProps['onAddTag'];
  onRemoveTag?: TagsGroupProps['onRemoveTag'];
}

const SingleFileInfo: React.FC<Props> = ({ file, onAddTag, onRemoveTag }) => {
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
      <TagsGroup tags={file.tags} onAddTag={onAddTag} onRemoveTag={onRemoveTag} />
      {metadataList}
    </>
  );
};

export default SingleFileInfo;
