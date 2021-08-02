import styled from '@emotion/styled';

const LEFT_SIZE = 240;
const RIGHT_SIZE = 240;

const Container = styled.div({
  width: '100vw',
  height: '100vh',
  overflow: 'hidden',
});

const LeftContainer = styled.div(({ theme }) => ({
  display: 'inline-block',
  background: theme.colors.secondBackground,
  overflow: 'hidden',
  height: '100%',
  width: LEFT_SIZE,
}));

const RightContainer = styled.div(({ theme }) => ({
  display: 'inline-block',
  background: theme.colors.secondBackground,
  overflow: 'hidden',
  height: '100%',
  width: LEFT_SIZE,
}));

const CenterContainer = styled.div(({ theme }) => ({
  display: 'inline-block',
  background: theme.colors.background,
  overflow: 'hidden',
  height: '100%',
  width: `calc(100% - ${LEFT_SIZE}px - ${RIGHT_SIZE}px)`,
}));

const ToolBarContainer = styled.div({
  marginTop: '8px',
});

interface IAppLayoutProps {
  left: React.ReactNode;
  tooltip: React.ReactNode;
  center: React.ReactNode;
  right: React.ReactNode;
}

const AppLayout: React.FunctionComponent<IAppLayoutProps> = ({ left, tooltip, center, right }) => {
  return (
    <Container>
      <LeftContainer>{left}</LeftContainer>
      <CenterContainer>
        <ToolBarContainer>{tooltip}</ToolBarContainer>
        {center}
      </CenterContainer>
      <RightContainer>{right}</RightContainer>
    </Container>
  );
};

export default AppLayout;
