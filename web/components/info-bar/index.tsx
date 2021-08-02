import styled from '@emotion/styled';
import { Bit, ColorSpace, Edit, Empty, Rotate, Size, Time } from 'components/icons';
import InfoBarField from './field';
import FileInfo from './file-info';
import InfoBarGroup from './group';
import InfoBarHeader from './header';

interface Props {
  className?: string;
}

const Container = styled.div(({ theme }) => ({
  padding: '8px 16px',
  color: theme.colors.gray100,
  '& > *': {
    marginBottom: '16px',
  },
}));

const InfoBar: React.FC<Props> = ({ className }) => (
  <Container className={className}>
    <InfoBarHeader />
    <FileInfo />
    <InfoBarGroup title="Information">
      <InfoBarField icon={Time} name="Create Time" value="9/3/20 13:36:57" />
      <InfoBarField icon={Edit} name="Modify time" value="9/3/20 13:36:57" />
    </InfoBarGroup>
    <InfoBarGroup title="Image">
      <InfoBarField icon={Size} name="Size" value="1920x1080" />
      <InfoBarField icon={Bit} name="Bit Depth" value="24 Bits" />
      <InfoBarField icon={ColorSpace} name="Color Space" value="sRGB" />
      <InfoBarField icon={Rotate} name="Direction" value="Rotate 270°" />
      <InfoBarField icon={Empty} name="Gamma" value="2.5" />
    </InfoBarGroup>
  </Container>
);

export default InfoBar;
