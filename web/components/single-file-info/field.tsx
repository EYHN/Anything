import styled from '@emotion/styled';
import { Empty } from 'components/icons';
import { useI18n } from 'i18n';
import { MetadataEntry, MetadataIcon } from 'metadata';

interface Props {
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

const MetadataEntryInfo: React.FC<Props> = ({ className, entry }) => {
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

export default MetadataEntryInfo;
