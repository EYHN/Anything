import styled from '@emotion/styled';

interface Props {
  className?: string;
  title?: React.ReactNode;
}

const Container = styled.div({
  '& > *': {
    marginBottom: '16px',
  },
});

const Header = styled.h4({ margin: '0 0 16px', paddingTop: '8px', fontWeight: 400, fontSize: '12px' });

const InfoBarGroup: React.FC<Props> = ({ className, children, title }) => (
  <Container className={className}>
    {title && <Header>{title}</Header>}
    {children}
  </Container>
);

export default InfoBarGroup;
