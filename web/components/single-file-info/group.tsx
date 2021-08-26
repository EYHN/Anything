import styled from '@emotion/styled';
import { useI18n } from 'i18n';
import { MetadataEntry, MetadataGroupKey } from 'metadata';
import { useCallback, useMemo, useState } from 'react';
import MetadataEntryInfo from './field';

interface Props {
  className?: string;
  groupKey: MetadataGroupKey;
  entries: MetadataEntry[];
}

const Container = styled.div({
  '& > *': {
    marginBottom: '16px',
  },
});

const Header = styled.h4({ margin: '0 0 16px', paddingTop: '8px', fontWeight: 400, fontSize: '12px' });

const ShowAction = styled.span(({ theme }) => ({
  margin: '0 0 16px',
  paddingTop: '8px',
  fontWeight: 400,
  fontSize: '12px',
  float: 'right',
  color: theme.colors.gray300,
  userSelect: 'none',
  cursor: 'pointer',
}));

const MetadataEntryGroup: React.FC<Props> = ({ className, entries, groupKey }) => {
  const { localeMetadata, localeUI } = useI18n();
  const [isShowMore, showMore] = useState<boolean>(false);

  const hasMore = useMemo(() => entries.some((entry) => entry.advanced), [entries]);
  const children = useMemo(() => entries.filter((entry) => !entry.advanced || isShowMore), [entries, isShowMore]);

  const handleClickAction = useCallback(() => showMore(!isShowMore), [isShowMore]);

  return (
    <Container className={className}>
      {hasMore && (
        <>
          <ShowAction onClick={handleClickAction}>
            {isShowMore ? localeUI('UI.Metadata.ShowLess') : localeUI('UI.Metadata.ShowMore')}
          </ShowAction>
        </>
      )}
      <Header>{localeMetadata(groupKey)}</Header>
      {children.map((entry) => (
        <MetadataEntryInfo key={entry.key} entry={entry} />
      ))}
    </Container>
  );
};

export default MetadataEntryGroup;
