import { useFileInfoQuery } from 'api';
import MetadataBarDictionarySection from 'components/MetadataBar/Section/Dictionary';
import MetadataBarFileHeader from 'components/MetadataBar/FileHeader';
import MetadataBarLayout from 'components/MetadataBar/Layout';
import React from 'react';
import { useI18n } from 'i18n';
import { parseMetadataDictionary } from 'utils/Metadata';
import MetadataBarPaletteSection from 'components/MetadataBar/Section/Palette';

interface Props {
  url: string;
}

const SingleFileMetadataBar: React.FunctionComponent<Props> = ({ url }) => {
  const { error, data } = useFileInfoQuery({ variables: { url }, fetchPolicy: 'cache-and-network' });
  const { localeMetadata, localeMetadataValue } = useI18n();

  if (error) return <p>Error :({error.message}</p>;

  if (data && data.file.__typename === 'RegularFile') {
    const parsedMetadata = parseMetadataDictionary(data.file.metadata);
    const { Palette: paletteMetadata, ...otherMetadata } = parsedMetadata;

    return (
      <MetadataBarLayout>
        <MetadataBarFileHeader file={data.file}></MetadataBarFileHeader>
        {paletteMetadata && paletteMetadata[0] && <MetadataBarPaletteSection colors={paletteMetadata[0].value.toString().split(',')} />}
        {Object.keys(otherMetadata).map((sectionName) => {
          const localizedDictionary = otherMetadata[sectionName].map((item) => ({
            key: localeMetadata(item.fullKey, item.key),
            value: localeMetadataValue(item.fullKey, item.value, item.value.toString() || ' '),
            attributes: item.attributes,
          }));
          const normal = localizedDictionary.filter((item) => !item.attributes.includes('Advanced'));
          const advanced = localizedDictionary.filter((item) => item.attributes.includes('Advanced'));
          const localeSectionTitle = localeMetadata(sectionName, sectionName);
          return (
            <MetadataBarDictionarySection
              key={url + sectionName}
              title={localeSectionTitle}
              dictionary={normal}
              extraDictionary={advanced}
            ></MetadataBarDictionarySection>
          );
        })}
      </MetadataBarLayout>
    );
  }

  return <></>;
};

export default SingleFileMetadataBar;
