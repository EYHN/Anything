import React, { memo } from 'react';
import { IDirentFragment } from 'api';
import FileThumbnail from 'components/file-icons';
import styled from '@emotion/styled';
import { useI18n } from 'i18n';

interface FileProps {
  dirent: IDirentFragment;
  height: number;
  width: number;
  selecting: boolean;
  className?: string;
  style?: React.CSSProperties;
  onDoubleClick?: React.MouseEventHandler;
  onMouseDown?: React.MouseEventHandler;
}

const topPadding = 8;
const bottomPadding = 10;
const leftPadding = 4;
const rightPadding = 4;
const textHeight = 67;

const Container = styled.div<{ selected: boolean }>(({ selected }) => ({
  position: 'relative',
  borderRadius: '5px',
  background: !selected ? 'transparent' : 'rgba(0,0,0,.1)',
  '&:hover': {
    background: !selected ? 'rgba(0,0,0,.05)' : undefined,
  },
}));

const InnerContainer = styled.div({
  padding: `${topPadding}px ${rightPadding}px ${bottomPadding}px ${leftPadding}px`,
  '& > *': {
    pointerEvents: 'none',
  },
});

const TextContainer = styled.div({
  width: '100%',
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

const LastWriteTime = styled.p(({ theme }) => ({
  fontSize: '12px',
  textAlign: 'center',
  color: theme.colors.gray200,
  margin: '0',
}));

const File: React.FunctionComponent<FileProps> = memo(
  ({ dirent, selecting, width, height, className, style, onDoubleClick, onMouseDown }) => {
    const { i18n } = useI18n();
    const imageSize = Math.min(height - topPadding - bottomPadding - textHeight, width - leftPadding - rightPadding);
    const imageLeft = (width - leftPadding - rightPadding - imageSize) / 2;
    const imageTopBottomMargin = (height - topPadding - bottomPadding - textHeight - imageSize) / 2;

    return (
      <Container selected={selecting} className={className} style={{ width, ...style }}>
        <InnerContainer draggable>
          <FileThumbnail
            file={dirent.file}
            width={imageSize}
            height={imageSize}
            style={{ marginLeft: imageLeft, marginRight: imageLeft, marginTop: imageTopBottomMargin, marginBottom: imageTopBottomMargin }}
            onDoubleClick={onDoubleClick}
            onMouseDown={onMouseDown}
          />
          <TextContainer>
            <FileName>{dirent.name}</FileName>
            <LastWriteTime>{dirent.file.stats.lastWriteTime && i18n.date(dirent.file.stats.lastWriteTime)}</LastWriteTime>
          </TextContainer>
        </InnerContainer>
      </Container>
    );
  },
);

File.displayName = 'File';

export default File;
