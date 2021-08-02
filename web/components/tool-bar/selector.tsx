import { useTheme } from '@emotion/react';
import styled from '@emotion/styled';

interface Props {
  name: React.ReactNode;
  icon: React.ElementType<React.SVGProps<SVGSVGElement>>;
}

const Container = styled.div({
  display: 'inline-flex',
  height: '40px',
  alignItems: 'center',
});

const Name = styled.span(({ theme }) => ({
  fontSize: '12px',
  padding: '0 6px',
  color: theme.colors.gray200,
}));

const ToolBarSelector: React.FC<Props> = ({ icon: Icon, name }) => {
  const theme = useTheme();
  return (
    <Container>
      <Icon color={theme.colors.gray300} fontSize="18px" />
      <Name>{name}</Name>
      <svg width="10" height="6">
        <path d="M1 1L5 5L9 1" stroke={theme.colors.gray300} fill="none" strokeWidth="1.3" strokeLinecap="round" strokeLinejoin="round" />
      </svg>
    </Container>
  );
};

export default ToolBarSelector;
