import styled from '@emotion/styled';

const InfoBarLayout = styled.div(({ theme }) => ({
  padding: '8px 16px',
  color: theme.colors.gray100,
  '& > *': {
    marginBottom: '16px',
  },
}));

export default InfoBarLayout;
