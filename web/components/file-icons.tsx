import React from 'react';
import SizedImage, { Match } from 'components/sized-image';
import { IFileFragment } from 'api';

interface FileProps extends React.ImgHTMLAttributes<HTMLImageElement> {
  file: IFileFragment;
  height: number;
  width: number;
  dropShadow?: boolean;
  style?: React.CSSProperties;
  className?: string;
}

const FileThumbnail = React.forwardRef<HTMLImageElement, FileProps>(({ file, width, height, ...otherProps }, ref) => {
  const url = new URL(file.thumbnail || file.icon, window.location.href);

  const imageMatchlist: Match[] = [
    {
      width: 64,
      height: 64,
      src: (url.searchParams.set('size', '64'), url.href),
    },
    {
      width: 128,
      height: 128,
      src: (url.searchParams.set('size', '128'), url.href),
    },
    {
      width: 256,
      height: 256,
      src: (url.searchParams.set('size', '256'), url.href),
    },
    {
      width: 512,
      height: 512,
      src: (url.searchParams.set('size', '512'), url.href),
    },
    {
      width: 1024,
      height: 1024,
      src: (url.searchParams.set('size', '1024'), url.href),
    },
  ];

  return <SizedImage width={width} height={height} matchlist={imageMatchlist} ref={ref} {...otherProps} />;
});

FileThumbnail.displayName = 'FileIcon';

export default FileThumbnail;
