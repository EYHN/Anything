import IconButton from 'components/Icons/IconButton';
import Add from 'components/Icons/SideBar/Add';
import SideBarIcon from 'components/Icons/SideBar/SideBar';
import Menu from 'components/Icons/SideBar/Menu';
import React from 'react';
import { createUseStyles } from 'react-jss';
import classNames from 'classnames';
import QuickAccesses from './QuickAccesses';

const useStyles = createUseStyles({
  sideBar: {
    display: 'flex',
    flexDirection: 'column',
    padding: '12px 0',
    height: '100%'
  },
  sideBarItem: {
    marginBottom: '12px',
    "&:last-child": {
      marginBottom: '0'
    }
  },
  filling: {
    flexGrow: 1,
    overflow: 'hidden'
  },
  activitys: {
    display: 'flex',
    flexDirection: 'row',
    padding: '0 12px'
  },
  quickAccesses: {
    padding: '0 16px'
  }
})

const SideBar: React.FunctionComponent<{}> = () => {
  const classes = useStyles();

  return <div className={classes.sideBar}>
    <div className={classNames(classes.activitys, classes.sideBarItem)}>
      <IconButton icon={Menu} color='rgba(0,0,0,0.55)' />
      <div className={classes.filling}></div>
      <IconButton icon={Add} color='rgba(0,0,0,0.40)' />
      <IconButton icon={SideBarIcon} color='rgba(0,0,0,0.40)' />
    </div>
    <div className={classNames(classes.sideBarItem, classes.quickAccesses)}>
      <QuickAccesses />
    </div>
  </div>
}

export default React.memo(SideBar);