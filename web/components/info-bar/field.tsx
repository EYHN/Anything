import styled from '@emotion/styled';

interface Props {
  className?: string;
  icon: React.ElementType<React.SVGProps<SVGSVGElement>>;
  name: React.ReactNode;
  value: React.ReactNode;
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

const InfoBarField: React.FC<Props> = ({ className, icon: Icon, name, value }) => (
  <Container className={className}>
    <IconContainer>
      <Icon />
    </IconContainer>
    <ContentContainer>
      <Name>{name}</Name>
      <Value>{value}</Value>
    </ContentContainer>
  </Container>
);

export default InfoBarField;
