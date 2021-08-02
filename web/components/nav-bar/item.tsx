import { useTheme } from '@emotion/react';
import styled from '@emotion/styled';

interface Props {
  className?: string;
  active?: boolean;
  icon: React.ElementType<React.SVGProps<SVGSVGElement>>;
  text: React.ReactNode;
}

const Container = styled.div<{ active?: boolean }>(({ active }) => ({
  display: 'flex',
  alignItems: 'center',
  padding: '0px 12px',
  height: '44px',
  lineHeight: '44px',
  background: active ? 'rgba(0,0,0,.05)' : undefined,
  borderRadius: '10px',
}));

const Text = styled.span({
  padding: '0 12px',
  fontSize: '13px',
  fontWeight: 400,
});

const NavBarItem: React.FC<Props> = ({ className, active, icon: Icon, text }) => {
  const theme = useTheme();
  return (
    <Container className={className} active={active}>
      <Icon fontSize="24px" color={theme.colors.gray200} />
      <Text>{text}</Text>
    </Container>
  );
};

export default NavBarItem;
