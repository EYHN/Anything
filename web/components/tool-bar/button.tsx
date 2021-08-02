import { useTheme } from '@emotion/react';
import styled from '@emotion/styled';

interface Props {
  className?: string;
  icon: React.ElementType<React.SVGProps<SVGSVGElement>>;
}

const Container = styled.div({
  height: '40px',
  display: 'inline-flex',
  alignItems: 'center',
});

const ToolBarButton: React.FC<Props> = ({ className, icon: Icon }) => {
  const theme = useTheme();

  return (
    <Container className={className}>
      <Icon color={theme.colors.gray300} fontSize="20px" />
    </Container>
  );
};

export default ToolBarButton;
