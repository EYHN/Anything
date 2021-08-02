import { createUseStyles } from 'react-jss';
import classNames from 'classnames';
import Directory from 'components/icons/quick-access/directory';

interface IQuickAccessesProps {
  className?: string;
}

const useStyles = createUseStyles({
  quickAccesses: {},
  title: {
    margin: 0,
    fontSize: '12px',
    lineHeight: '16px',
    color: 'rgba(0,0,0,0.8)',
    fontWeight: 'bold',
  },
  list: {
    listStyle: 'none',
    margin: '8px 0',
    padding: 0,
  },
  listItem: {
    display: 'flex',
    flexDirection: 'row',
    alignItems: 'center',
    height: '24px',
    marginBottom: '8px',
    padding: 0,
    position: 'relative',
    zIndex: 1,
  },
  activeItem: {
    '&:before': {
      content: '""',
      position: 'absolute',
      left: '-6px',
      top: '-3px',
      width: '100%',
      height: '100%',
      background: 'rgba(0,0,0,0.1)',
      boxSizing: 'content-box',
      borderRadius: '3px',
      padding: '3px 6px',
    },
  },
  itemIcon: {
    height: '20px',
    width: '20px',
    marginLeft: '2px',
    marginRight: '4px',
  },
  itemLabel: {
    fontSize: '13px',
    lineHeight: '20px',
  },
});

const QuickAccesses: React.FunctionComponent<IQuickAccessesProps> = ({ className }) => {
  const classes = useStyles();

  return (
    <div className={classNames(classes.quickAccesses, className)}>
      <h4 className={classes.title}>个人收藏</h4>
      <ul className={classes.list}>
        <li className={classes.listItem}>
          <Directory className={classes.itemIcon} color="#0871F5" />
          <span className={classes.itemLabel}>图片</span>
        </li>
        <li className={classes.listItem}>
          <Directory className={classes.itemIcon} color="#0871F5" />
          <span className={classes.itemLabel}>影片</span>
        </li>
        <li className={classes.listItem}>
          <Directory className={classes.itemIcon} color="#0871F5" />
          <span className={classes.itemLabel}>下载</span>
        </li>
        <li className={classNames(classes.listItem, classes.activeItem)}>
          <Directory className={classes.itemIcon} color="#0871F5" />
          <span className={classes.itemLabel}>音乐</span>
        </li>
      </ul>
    </div>
  );
};

export default QuickAccesses;
