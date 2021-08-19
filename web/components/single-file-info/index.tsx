import { IFileInfoFragment } from 'api';
import MetadataEntryInfo from './field';
import Title from './title';
import MetadataEntryGroup from './group';
import { useMemo } from 'react';
import { parseMetadataPayload } from 'metadata';

interface Props {
  file: IFileInfoFragment;
}

const SingleFileInfo: React.FC<Props> = ({ file }) => {
  const metadataList = useMemo(() => {
    const groupedMetadata = parseMetadataPayload(file.metadata);

    const groupKeys = Reflect.ownKeys(groupedMetadata) as (keyof typeof groupedMetadata)[];

    return groupKeys.map((groupKey) => {
      const items = groupedMetadata[groupKey];
      return (
        <MetadataEntryGroup key={groupKey} groupKey={groupKey}>
          {items?.map((item) => (
            <MetadataEntryInfo key={item.key} entry={item} />
          ))}
        </MetadataEntryGroup>
      );
    });
  }, [file.metadata]);

  return (
    <>
      <Title file={file} />
      {metadataList}
    </>
  );
};

export default SingleFileInfo;
