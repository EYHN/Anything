import Forward from 'components/Icons/ToolBar/Forward';
import React from 'react';
import { createUseStyles } from 'react-jss';
import Back from 'components/Icons/ToolBar/Back';
import ToolBarButton from './ToolBarButton';
import classNames from 'classnames';
import LayoutSwitcher from './LayoutSwitcher';
import Send from 'components/Icons/ToolBar/Send';
import Tags from 'components/Icons/ToolBar/Tags';
import SearchBar from './SearchBar';

const useStyles = createUseStyles({
  toolBar: {
    display: 'flex',
    minHeight: '48px',
    flexDirection: 'row',
    alignItems: 'center',
    padding: '8px'
  },
  toolBarItem: {
    display: 'flex',
    flexDirection: 'row',
    alignItems: 'center',
    marginRight: '8px',
    "&:last-child": {
      marginRight: '0'
    }
  },
  filling: {
    flexGrow: 1,
    overflow: 'hidden'
  },
  title: {
    display: 'inline-block',
    minWidth: '100px',
    fontSize: '14px',
    lineHeight: '20px',
    fontWeight: 'bold',
    color: '#4C4C4C',
    margin: '0px',
    whiteSpace: 'nowrap',
    overflow: 'hidden',
    textOverflow: 'ellipsis'
  }
});

const ToolBar: React.FunctionComponent<{}> = () => {
  const classes = useStyles();

  return <div className={classes.toolBar}>
    <div className={classes.toolBarItem}>
      <ToolBarButton icon={Back}/>
      <ToolBarButton icon={Forward} disabled/>
    </div>
    <h2 className={classNames(classes.toolBarItem, classes.filling, classes.title)}>
      Backup
    </h2>
    <div className={classes.toolBarItem}  style={{marginRight: '30px'}}>
      <LayoutSwitcher selected={'Grid'} />
    </div>

    <div className={classes.toolBarItem} style={{marginRight: '30px'}}>
      <ToolBarButton icon={Send}/>
      <ToolBarButton icon={Tags}/>
    </div>

    <div className={classes.toolBarItem}>
      <SearchBar />
    </div>
  </div>
}

export default React.memo(ToolBar);