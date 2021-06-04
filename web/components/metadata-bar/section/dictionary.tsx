import { Trans } from '@lingui/react';
import React, { useState } from 'react';
import { createUseStyles } from 'react-jss';

const useStyles = createUseStyles({
  root: {
    marginTop: '4px',
    marginBottom: '24px',
  },
  header: {
    marginBottom: '8px',
    display: 'flex',
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  title: {
    display: 'inline-block',
    margin: 0,
    flexShrink: 0,
    fontSize: '14px',
    lineHeight: 1.5,
    fontWeight: 600,
    color: '#000',
    whiteSpace: 'nowrap',
    overflow: 'hidden',
    textOverflow: 'ellipsis',
  },
  moreAnchor: {
    display: 'inline-block',
    textAlign: 'right',
    fontSize: '13px',
    color: 'rgba(0, 0, 0, 0.72)',
    marginLeft: 'auto',
    cursor: 'pointer',
    userSelect: 'none',
    whiteSpace: 'nowrap',
    overflow: 'hidden',
    textOverflow: 'ellipsis',
  },
  list: {
    listStyle: 'none',
    padding: 0,
    margin: 0,
    overflow: 'hidden',
    transition: '300ms height',
  },
  listItem: {
    fontSize: '13px',
    lineHeight: '18px',
    marginBottom: '8px',
    height: '18px',
  },
  itemKey: {
    display: 'inline-block',
    width: '50%',
    paddingRight: '4px',
    color: 'rgba(0, 0, 0, 0.9)',
    fontWeight: 400,
    whiteSpace: 'nowrap',
    overflow: 'hidden',
    textOverflow: 'ellipsis',
  },
  itemValue: {
    display: 'inline-block',
    width: '50%',
    paddingLeft: '4px',
    fontWeight: 400,
    textAlign: 'right',
    color: 'rgba(0, 0, 0, 0.72)',
    whiteSpace: 'nowrap',
    overflow: 'hidden',
    textOverflow: 'ellipsis',
  },
});

interface Props {
  title?: string;
  dictionary: { key: string; value: string }[];
  extraDictionary?: { key: string; value: string }[];
}

const MetadataBarDictionarySection: React.FunctionComponent<Props> = ({ title, dictionary, extraDictionary }) => {
  const styles = useStyles();
  const [showMore, setShowMore] = useState(false);
  const hasMore = extraDictionary && extraDictionary.length > 0;

  const handleClickMoreAnchor = React.useCallback(() => {
    return setShowMore((isShowMore) => !isShowMore);
  }, []);

  const listLength = showMore && hasMore && extraDictionary ? dictionary.length + extraDictionary?.length : dictionary.length;
  const listHeight = Math.max(18 * listLength + (listLength - 1) * 8, 0);

  return (
    <section className={styles.root}>
      <div className={styles.header}>
        {title && (
          <h5 title={title.toString()} className={styles.title}>
            {title}
          </h5>
        )}
        {hasMore && (
          <span role="button" onClick={handleClickMoreAnchor} className={styles.moreAnchor}>
            {showMore ? <Trans id="UI.Metadata.ShowLess" message="show less" /> : <Trans id="UI.Metadata.ShowMore" message="show more" />}
          </span>
        )}
      </div>
      <ul className={styles.list} style={{ height: listHeight }}>
        {dictionary.map(({ key, value }) => {
          return (
            <li className={styles.listItem} key={'key:' + key}>
              <span className={styles.itemKey}>{key}</span>
              <span title={value.toString()} className={styles.itemValue}>
                {value.toString()}
              </span>
            </li>
          );
        })}
        {hasMore &&
          extraDictionary &&
          extraDictionary.map(({ key, value }) => {
            return (
              <li className={styles.listItem} key={'extrakey:' + key}>
                <span title={key.toString()} className={styles.itemKey}>
                  {key}
                </span>
                <span title={value.toString()} className={styles.itemValue}>
                  {value.toString()}
                </span>
              </li>
            );
          })}
      </ul>
    </section>
  );
};

export default MetadataBarDictionarySection;
