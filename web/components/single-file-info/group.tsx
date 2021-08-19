import styled from '@emotion/styled';
import { useI18n } from 'i18n';
import { MetadataGroupKey } from 'metadata';

interface Props {
  className?: string;
  groupKey: MetadataGroupKey;
}

const Container = styled.div({
  '& > *': {
    marginBottom: '16px',
  },
});

const Header = styled.h4({ margin: '0 0 16px', paddingTop: '8px', fontWeight: 400, fontSize: '12px' });

const MetadataEntryGroup: React.FC<Props> = ({ className, children, groupKey }) => {
  const { localeMetadata } = useI18n();

  return (
    <Container className={className}>
      <Header>{localeMetadata(groupKey)}</Header>
      {children}
    </Container>
  );
};

export default MetadataEntryGroup;
