import React, { memo } from 'react';
import { IFileFragment } from 'api';
import FileIcon from 'components/file-icons';
import styled from '@emotion/styled';

interface FileProps {
  file: IFileFragment;
  height: number;
  width: number;
  focus: boolean;
  className?: string;
  style?: React.CSSProperties;
  onDoubleClick?: React.MouseEventHandler;
  onMouseDown?: React.MouseEventHandler;
}

const topPadding = 8;
const bottomPadding = 10;
const leftPadding = 4;
const rightPadding = 4;
const textHeight = 60;

const Container = styled.div({
  position: 'relative',
  borderRadius: '5px',
  background: 'transparent',
  '&:hover': {
    background: 'rgba(0,0,0,.05)',
  },
});

const InnerContainer = styled.div({
  padding: `${topPadding}px ${rightPadding}px ${bottomPadding}px ${leftPadding}px`,
  '& > *': {
    pointerEvents: 'none',
  },
});

const TextContainer = styled.div({
  width: '100%',
  paddingTop: '4px',
});

const FileName = styled.p(({ theme }) => ({
  display: '-webkit-box',
  WebkitLineClamp: 2,
  WebkitBoxOrient: 'vertical',
  lineHeight: '1.5',
  fontSize: '14px',
  textOverflow: 'ellipsis',
  overflowWrap: 'break-word',
  overflow: 'hidden',
  margin: '0 0 2px',
  textAlign: 'center',
  color: theme.colors.gray100,
}));

const File: React.FunctionComponent<FileProps> = memo(({ file, width, height, className, style, onDoubleClick, onMouseDown }) => {
  const imageSize = Math.min(height - topPadding - bottomPadding - textHeight, width - leftPadding - rightPadding);
  const imageLeft = (width - leftPadding - rightPadding - imageSize) / 2;
  const textTopMargin = height - topPadding - bottomPadding - textHeight - imageSize;

  return (
    <Container className={className} style={{ width, ...style }}>
      <InnerContainer draggable>
        <FileIcon
          file={file}
          width={imageSize}
          height={imageSize}
          style={{ marginLeft: imageLeft, marginRight: imageLeft }}
          onDoubleClick={onDoubleClick}
          onMouseDown={onMouseDown}
        />
        <TextContainer style={{ marginTop: textTopMargin }}>
          <FileName>{file.name}</FileName>
        </TextContainer>
      </InnerContainer>
    </Container>
  );
});

File.displayName = 'File';

export default File;
