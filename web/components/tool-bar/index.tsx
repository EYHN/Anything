import { useTheme } from '@emotion/react';
import styled from '@emotion/styled';
import { Filter, List, Resize, Search, Sort } from 'components/icons';
import Breadcrumb from './breadcrumb';
import ToolBarButton from './button';
import ToolBarRange from './range';
import ToolBarSelector from './selector';

interface Props {
  className?: string;
}

const Container = styled.div({
  padding: '0px 40px',
});

const Line1 = styled.div({
  display: 'flex',
  justifyContent: 'space-between',
  alignItems: 'center',
  height: '52px',
});

const Line2 = styled.div({});

const Line2Left = styled.div({
  display: 'inline-block',
  '& > *': {
    marginRight: '16px',
  },
});

const Line2Right = styled.div({
  float: 'right',
  '& > *': {
    marginLeft: '16px',
  },
});

const ToolBar: React.FC<Props> = ({ className }) => {
  const theme = useTheme();
  return (
    <Container className={className}>
      <Line1>
        <Breadcrumb />
        <Search fontSize="20px" color={theme.colors.gray200} />
      </Line1>
      <Line2>
        <Line2Left>
          <ToolBarSelector icon={Sort} name="Name" />
          <ToolBarSelector icon={Filter} name="Filter" />
        </Line2Left>
        <Line2Right>
          <ToolBarRange icon={Resize} />
          <ToolBarButton icon={List} />
        </Line2Right>
      </Line2>
    </Container>
  );
};

export default ToolBar;
