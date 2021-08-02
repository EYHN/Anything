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

const ToolBarRange: React.FC<Props> = ({ icon: Icon, className }) => {
  const theme = useTheme();
  return (
    <Container className={className}>
      <Icon color={theme.colors.gray300} fontSize="20px" />
    </Container>
  );
};

export default ToolBarRange;
