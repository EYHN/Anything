import styled from '@emotion/styled';
import { Storage, Switch } from 'components/icons';

interface Props {
  className?: string;
}

const Container = styled.div({
  display: 'flex',
  alignItems: 'center',
  height: 52,
  padding: '0 12px 0 10px',
});

const LibraryIcon = styled.div(({ theme }) => ({
  display: 'flex',
  justifyContent: 'center',
  alignItems: 'center',
  width: 28,
  height: 28,
  fontSize: '16px',
  background: theme.colors.gray200,
  borderRadius: '50%',
  color: theme.colors.gray400,
}));

const LibraryName = styled.span({
  fontSize: '13px',
  lineHeight: 1.6,
  fontWeight: 400,
  flexGrow: 1,
  padding: '0px 10px',
});

const SwitchLibraryButton = styled(Switch)(({ theme }) => ({
  fontSize: '16px',
  color: theme.colors.gray200,
}));

const NavBarHeader: React.FC<Props> = ({ className }) => (
  <Container className={className}>
    <LibraryIcon>
      <Storage />
    </LibraryIcon>
    <LibraryName>Local Library</LibraryName>
    <SwitchLibraryButton role="button" />
  </Container>
);

export default NavBarHeader;
