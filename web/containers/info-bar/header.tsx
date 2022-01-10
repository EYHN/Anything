import { useTheme } from '@emotion/react';
import styled from '@emotion/styled';
import { RightBar } from 'components/icons';

interface Props {
  className?: string;
}

const Container = styled.div({
  display: 'flex',
  justifyContent: 'space-between',
  alignItems: 'center',
  height: '52px',
  padding: '16px',
});

const Title = styled.h3({
  margin: '0px',
  fontSize: '14px',
  fontWeight: 400,
});

const InfoBarHeader: React.FC<Props> = ({ className }) => {
  const theme = useTheme();

  return (
    <Container className={className}>
      <Title>Details</Title>
      <RightBar color={theme.colors.gray200} fontSize="18px" />
    </Container>
  );
};

export default InfoBarHeader;
