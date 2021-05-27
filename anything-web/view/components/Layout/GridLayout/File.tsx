import React from 'react';
import { createUseStyles } from 'react-jss';
import { IFileFragment } from 'api';
import FileIcon from 'components/FileIcons';

interface FileProps {
  file: IFileFragment;
  height: number;
  width: number;
  focus: boolean;
  style?: React.CSSProperties;
  onDoubleClick?: React.MouseEventHandler;
  onMouseDown?: React.MouseEventHandler;
  imgRef?: React.RefObject<HTMLImageElement>;
  textRef?: React.RefObject<HTMLElement>;
}

interface StyleProps {
  imageSize: number;
  imageLeft: number;
  imageTop: number;
  textHeight: number;
  focus: boolean;
}

const useStyles = createUseStyles({
  image: ({ imageSize, imageLeft, imageTop, focus }: StyleProps) => ({
    position: 'absolute',
    width: imageSize,
    height: imageSize,
    left: imageLeft,
    top: imageTop,
    ...(focus && {
      background: 'rgba(0, 0, 0, 0.07)',
      borderRadius: `${(imageSize / 100) * 5}px`,
    }),
  }),
  textStyle: ({ textHeight }: StyleProps) => ({
    display: 'inline-block',
    padding: '0px 10px 10px',
    height: textHeight,
    position: 'absolute',
    left: 0,
    bottom: 0,
    textAlign: 'center',
    boxSizing: 'border-box',
    width: '100%',
    wordBreak: 'break-all',
  }),
  spanStyle: ({ focus }: StyleProps) => ({
    WebkitBoxDecorationBreak: 'clone',
    borderRadius: '3px',
    padding: '1px 2px',
    lineHeight: '1.3',
    fontSize: '14px',
    color: !focus ? '#000' : '#fff',
    backgroundColor: !focus ? 'transparent' : '#0363E2',
  }),
});

const File: React.FunctionComponent<FileProps> = ({ file, width, height, focus, style, onDoubleClick, onMouseDown, imgRef, textRef }) => {
  const containerPadding = 10;
  const textHeight = 60;
  const imageSize = Math.min(height - containerPadding - textHeight, width - containerPadding * 2);
  const imageLeft = (width - imageSize) / 2;
  const imageTop = (height - textHeight - imageSize) / 2;

  const classes = useStyles({
    imageSize: imageSize,
    imageLeft: imageLeft,
    imageTop: imageTop,
    textHeight: textHeight,
    focus: focus,
  });

  return (
    <div style={{ position: 'absolute', ...style }}>
      <FileIcon
        file={file}
        width={imageSize}
        height={imageSize}
        className={classes.image}
        onDoubleClick={onDoubleClick}
        onMouseDown={onMouseDown}
        ref={imgRef}
      />
      <div className={classes.textStyle}>
        <span onDoubleClick={onDoubleClick} onMouseDown={onMouseDown} className={classes.spanStyle} ref={textRef}>
          {file.name}
        </span>
      </div>
    </div>
  );
};

export default File;
