import styled from '@emotion/styled';
import { Empty } from 'components/icons';
import { useI18n } from 'i18n';
import { MetadataEntry, MetadataGroupKey, MetadataIcon } from 'metadata';
import { useCallback, useMemo, useState } from 'react';
import { GroupContainer, GroupHeader, GroupShowAction } from './group';

interface FieldProps {
  className?: string;
  entry: MetadataEntry;
}

const Container = styled.div({
  display: 'flex',
});

const IconContainer = styled.div(({ theme }) => ({
  fontSize: '16px',
  color: theme.colors.gray200,
  marginRight: '14px',
  '& > *': {
    verticalAlign: 'top',
  },
}));

const ContentContainer = styled.div({
  overflow: 'hidden',
});

const Name = styled.h5({
  margin: '0 0 6px',
  fontSize: '13px',
  fontWeight: 400,
  lineHeight: '16px',
  overflow: 'hidden',
  whiteSpace: 'nowrap',
  textOverflow: 'ellipsis',
});

const Value = styled.p(({ theme }) => ({
  margin: 0,
  fontSize: '13px',
  color: theme.colors.gray300,
}));

const Field: React.FC<FieldProps> = ({ className, entry }) => {
  const { localeMetadata, localeMetadataValue } = useI18n();

  const { key, value } = entry;
  const Icon = MetadataIcon[key] ?? Empty;
  return (
    <Container className={className}>
      <IconContainer>
        <Icon />
      </IconContainer>
      <ContentContainer>
        <Name>{localeMetadata(key)}</Name>
        <Value>{localeMetadataValue(key, value)}</Value>
      </ContentContainer>
    </Container>
  );
};

interface MetadataEntryGroupProps {
  className?: string;
  groupKey: MetadataGroupKey;
  entries: MetadataEntry[];
}

export const MetadataEntryGroup: React.FC<MetadataEntryGroupProps> = ({ className, entries, groupKey }) => {
  const { localeMetadata, localeUI } = useI18n();
  const [isShowMore, showMore] = useState<boolean>(false);

  const hasMore = useMemo(() => entries.some((entry) => entry.advanced), [entries]);
  const children = useMemo(() => entries.filter((entry) => !entry.advanced || isShowMore), [entries, isShowMore]);

  const handleClickAction = useCallback(() => showMore(!isShowMore), [isShowMore]);

  return (
    <GroupContainer className={className}>
      {hasMore && (
        <>
          <GroupShowAction onClick={handleClickAction}>
            {isShowMore ? localeUI('UI.Metadata.ShowLess') : localeUI('UI.Metadata.ShowMore')}
          </GroupShowAction>
        </>
      )}
      <GroupHeader>{localeMetadata(groupKey)}</GroupHeader>
      {children.map((entry) => (
        <Field key={entry.key} entry={entry} />
      ))}
    </GroupContainer>
  );
};

export default MetadataEntryGroup;
