import styled from '@emotion/styled';
import { Archive, Folder, Tag, Time, Trash } from 'components/icons';
import NavBarGroupHeader from './group-header';
import NavBarHeader from './header';
import Item from './item';

const Container = styled.div(({ theme }) => ({
  padding: '8px 16px',
  color: theme.colors.gray100,
}));

const Header = styled(NavBarHeader)({
  marginBottom: '24px',
});

const GroupHeader = styled(NavBarGroupHeader)({
  marginTop: '16px',
  marginBottom: '8px',
});

const NavBar: React.FunctionComponent = () => {
  return (
    <Container>
      <Header />
      <Item icon={Archive} text="All Files" active />
      <Item icon={Time} text="Recent" />
      <Item icon={Tag} text="All Tags" />
      <Item icon={Trash} text="Recycle Bin" />
      <GroupHeader>Favorites</GroupHeader>
      <Item icon={Folder} text="Assets" />
      <Item icon={Folder} text="Music" />
      <Item icon={Folder} text="Assets" />
      {/* <div className={classNames(classes.activitys, classes.sideBarItem)}>
        <IconButton icon={Menu} color="rgba(0,0,0,0.55)" />
        <div className={classes.filling}></div>
        <IconButton icon={Add} color="rgba(0,0,0,0.40)" />
        <IconButton icon={SideBarIcon} color="rgba(0,0,0,0.40)" />
      </div>
      <div className={classNames(classes.sideBarItem, classes.quickAccesses)}>
        <QuickAccesses />
      </div> */}
    </Container>
  );
};

export default NavBar;
