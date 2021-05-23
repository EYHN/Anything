import React, { useCallback, useState } from 'react';
import { createUseStyles } from 'react-jss';
import NavigationButton from './ToolBarButton';
import GridLayout from 'components/Icons/ToolBar/GridLayout';
import ListLayout from 'components/Icons/ToolBar/ListLayout';
import ColumnLayout from 'components/Icons/ToolBar/ColumnLayout';
import GalleryLayout from 'components/Icons/ToolBar/GalleryLayout';

interface ILayoutSwitcherProps {
  selected: string | null;
}

const useStyles = createUseStyles({
  layoutSwitcher: {
    display: 'flex',
    flexDirection: 'row',
    alignItems: 'center',
  },
  delimiter: {
    height: '15px',
    width: '0px',
    borderLeft: '1px solid rgba(0, 0, 0, 0.07)',
  },
});

const LayoutSwitcher: React.FunctionComponent<ILayoutSwitcherProps> = ({ selected }) => {
  const classes = useStyles();

  const [hover, setHover] = useState<boolean>(false);

  const handleHover = useCallback(() => {
    setHover(true);
  }, [setHover]);

  const handleLeaveHover = useCallback(() => {
    setHover(false);
  }, [setHover]);

  return (
    <div className={classes.layoutSwitcher} onMouseOver={() => handleHover()} onMouseOut={() => handleLeaveHover()}>
      <NavigationButton icon={GridLayout} active={selected === 'Grid'} focus={hover} />

      <span className={classes.delimiter} style={{ visibility: hover ? 'hidden' : 'visible' }} />

      <NavigationButton icon={ListLayout} active={selected === 'List'} focus={hover} />

      <span className={classes.delimiter} style={{ visibility: hover ? 'hidden' : 'visible' }} />

      <NavigationButton icon={ColumnLayout} active={selected === 'Column'} focus={hover} />

      <span className={classes.delimiter} style={{ visibility: hover ? 'hidden' : 'visible' }} />

      <NavigationButton icon={GalleryLayout} active={selected === 'Gallery'} focus={hover} />
    </div>
  );
};

export default LayoutSwitcher;
