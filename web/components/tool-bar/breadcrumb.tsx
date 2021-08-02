import styled from '@emotion/styled';

interface Props {
  className?: string;
}

const Container = styled.div({
  fontSize: '16px',
});

const Part = styled.span<{ active?: boolean }>(({ active, theme }) => ({
  color: active ? theme.colors.gray100 : theme.colors.gray200,
  marginRight: '8px',
}));

const Breadcrumb: React.FC<Props> = ({ className }) => (
  <Container className={className}>
    <Part>Files</Part>
    <Part>›</Part>
    <Part active>Assets</Part>
  </Container>
);

export default Breadcrumb;
