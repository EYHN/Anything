import React from 'react';
import { createUseStyles } from 'react-jss';

const useStyles = createUseStyles({
  root: {
    marginTop: '4px',
    marginBottom: '24px',
    display: 'flex',
    flexDirection: 'row',
    justifyContent: 'center',
  },
  color: {
    maxWidth: '28px',
    height: '13px',
    flex: 1,
    marginRight: '1px',
    '&:hover': {
      cursor: 'pointer',
      boxShadow: '0 0 0 1px #000',
    },
    '&:first-of-type': {
      borderTopLeftRadius: '2px',
      borderBottomLeftRadius: '2px',
    },
    '&:last-of-type': {
      borderTopRightRadius: '2px',
      borderBottomRightRadius: '2px',
    },
  },
});

interface Props {
  colors: string[];
}

const MetadataBarPaletteSection: React.FunctionComponent<Props> = ({ colors }) => {
  const styles = useStyles();

  return (
    <section className={styles.root}>
      {colors.slice(0, 8).map((color, index) => (
        <div className={styles.color} style={{ backgroundColor: color }} key={index}></div>
      ))}
    </section>
  );
};

export default MetadataBarPaletteSection;
