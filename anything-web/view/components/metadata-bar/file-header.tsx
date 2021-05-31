import { IFileInfoFragment } from 'api';
import FileIcon from 'components/file-icons';
import { useI18n } from 'i18n';
import React from 'react';
import { createUseStyles } from 'react-jss';
import fileSize from 'utils/filesize';

const useStyles = createUseStyles({
  container: {
    display: 'flex',
    justifyContent: 'start',
    alignItems: 'center',
    marginLeft: '-8px',
    marginRight: '-8px',
    marginBottom: '16px',
  },
  right: {
    display: 'block',
    marginLeft: '4px',
    overflow: 'hidden',
  },
  title: {
    margin: 0,
    color: '#000',
    fontSize: '16px',
    fontWeight: 600,
    lineHeight: 1.5,
    whiteSpace: 'nowrap',
    overflow: 'hidden',
    textOverflow: 'ellipsis',
  },
  subtitle: {
    display: 'flex',
    flexDirection: 'row',
    margin: 0,
    color: 'rgba(0, 0, 0, 0.6)',
    fontSize: '14px',
    fontWeight: 600,
    lineHeight: 1.5,
  },
  kindname: {
    whiteSpace: 'nowrap',
    overflow: 'hidden',
    textOverflow: 'ellipsis',
  },
  filesize: {
    flexShrink: 0,
  },
});

interface Props {
  file: IFileInfoFragment;
}

const MetadataBarFileHeader: React.FunctionComponent<Props> = ({ file }) => {
  const styles = useStyles();
  const { localeMimetype } = useI18n();

  const mime = localeMimetype(file.mime || 'other');

  // Shorten the file name when the width is not enough.
  // 'hello world.txt' => 'hello wo…txt'
  // JSON.stringify is for escape the string.
  const textOverflow = JSON.stringify('…' + file.name.substring(file.name.length - 3));

  return (
    <div className={styles.container}>
      <FileIcon file={file} width={90} height={90} />
      <div className={styles.right}>
        <h4 title={file.name} style={{ textOverflow: textOverflow }} className={styles.title}>
          {file.name}
        </h4>
        <p className={styles.subtitle}>
          <span title={mime} className={styles.kindname}>
            {mime}&nbsp;-&nbsp;
          </span>
          <span className={styles.filesize}>{fileSize(file.stats.size)}</span>
        </p>
      </div>
    </div>
  );
};

export default MetadataBarFileHeader;
