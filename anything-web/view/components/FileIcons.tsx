import React from 'react';
import SizedImage, { Match } from 'components/SizedImage';
import { IFile } from 'api';

interface FileProps extends React.ImgHTMLAttributes<HTMLImageElement> {
  file: Pick<IFile, 'dynamicIcon' | 'icon'>;
  height: number;
  width: number;
  style?: React.CSSProperties;
  className?: string;
}

const FileIcon = React.forwardRef<HTMLImageElement, FileProps>(({ file, width, height, ...otherProps }, ref) => {
  const url = new URL(file.dynamicIcon || file.icon, window.location.href);

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

export default FileIcon;
