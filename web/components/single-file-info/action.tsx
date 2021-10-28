import { useTheme } from '@emotion/react';
import styled from '@emotion/styled';
import React from 'react';

const Container = styled.span({
  display: 'inline-block',
  cursor: 'pointer',
  height: '20px',
  lineHeight: '20px',
});

const Label = styled.span(({ theme }) => ({
  display: 'inline-block',
  padding: '3px 0',
  marginLeft: '4px',
  fontSize: '12px',
  lineHeight: '14px',
  color: theme.colors.gray300,
}));

interface ActionProps {
  label?: string | null;
  icon: React.ElementType<React.SVGProps<SVGSVGElement>>;
  onClick?: React.MouseEventHandler;
}

const Action: React.VFC<ActionProps> = ({ label, icon: Icon, onClick }) => {
  const theme = useTheme();

  return (
    <Container onClick={onClick}>
      <Icon style={{ color: theme.colors.gray300, width: '20px', height: '20px', padding: '2px' }} />
      {label && <Label>{label}</Label>}
    </Container>
  );
};

export default Action;
