import styled from '@emotion/styled';

export const GroupContainer = styled.div({
  '& > *': {
    marginBottom: '16px',
  },
});

export const GroupHeader = styled.h4({ margin: '0 0 16px', paddingTop: '8px', fontWeight: 400, fontSize: '12px' });

export const GroupShowAction = styled.span(({ theme }) => ({
  margin: '0 0 16px',
  paddingTop: '8px',
  fontWeight: 400,
  fontSize: '12px',
  float: 'right',
  color: theme.colors.gray300,
  userSelect: 'none',
  cursor: 'pointer',
}));
