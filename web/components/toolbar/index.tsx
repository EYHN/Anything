import Forward from 'components/icons/toolbar/forward';
import React from 'react';
import { createUseStyles } from 'react-jss';
import Back from 'components/icons/toolbar/back';
import ToolBarButton from './toolbar-button';
import classNames from 'classnames';
import LayoutSwitcher from './layout-switcher';
import Send from 'components/icons/toolbar/send';
import Tags from 'components/icons/toolbar/tags';
import SearchBar from './search-bar';

const useStyles = createUseStyles({
  toolBar: {
    display: 'flex',
    minHeight: '48px',
    flexDirection: 'row',
    alignItems: 'center',
    padding: '8px',
  },
  toolBarItem: {
    display: 'flex',
    flexDirection: 'row',
    alignItems: 'center',
    marginRight: '8px',
    '&:last-child': {
      marginRight: '0',
    },
  },
  filling: {
    flexGrow: 1,
    overflow: 'hidden',
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
    textOverflow: 'ellipsis',
  },
});

const ToolBar: React.FunctionComponent = () => {
  const classes = useStyles();

  return (
    <div className={classes.toolBar}>
      <div className={classes.toolBarItem}>
        <ToolBarButton icon={Back} />
        <ToolBarButton icon={Forward} disabled />
      </div>
      <h2 className={classNames(classes.toolBarItem, classes.filling, classes.title)}>Backup</h2>
      <div className={classes.toolBarItem} style={{ marginRight: '30px' }}>
        <LayoutSwitcher selected={'Grid'} />
      </div>

      <div className={classes.toolBarItem} style={{ marginRight: '30px' }}>
        <ToolBarButton icon={Send} />
        <ToolBarButton icon={Tags} />
      </div>

      <div className={classes.toolBarItem}>
        <SearchBar />
      </div>
    </div>
  );
};

export default React.memo(ToolBar);
