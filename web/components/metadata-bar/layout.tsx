import { createUseStyles } from 'react-jss';

const useStyles = createUseStyles({
  container: {
    padding: '24px',
  },
});

const MetadataBarLayout: React.FC = ({ children }) => {
  const styles = useStyles();

  return <div className={styles.container}>{children}</div>;
};

export default MetadataBarLayout;
