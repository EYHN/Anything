import { forwardRef, memo } from 'react';
import styled from '@emotion/styled';
import usePixelRatio from './use-pixel-ratio';

interface SizedImageProps extends React.ImgHTMLAttributes<HTMLImageElement> {
  width: number;
  height: number;
  dropShadow?: boolean;
  matchlist?: Match[];
}

export interface Match {
  width: number;
  height: number;
  src: string;
}

const Image = styled.img<{ dropShadow?: boolean }>(({ dropShadow = false }) => ({
  imageRendering: '-webkit-optimize-contrast',
  filter: dropShadow ? 'drop-shadow(0 3px 5px rgba(0,0,0,0.2))' : '',
}));

function MatchSrc(matchlist: Match[], width: number, height: number) {
  matchlist.sort((a, b) => b.width * b.height - a.width * a.height);

  let result = matchlist[0].src;
  let resultpixelnum = matchlist[0].width * matchlist[0].height;

  for (const match of matchlist) {
    if (width <= match.width && height <= match.height && match.width * match.height < resultpixelnum) {
      result = match.src;
      resultpixelnum = match.width * match.height;
    }
  }

  return result;
}

const SizedImage = forwardRef<HTMLImageElement, SizedImageProps>(({ width, height, matchlist, src, ...otherprops }, ref) => {
  const pixelRatio = usePixelRatio();

  const finalsrc = matchlist ? MatchSrc(matchlist, width * pixelRatio, height * pixelRatio) : src;

  return <Image width={width} height={height} ref={ref} src={finalsrc} {...otherprops} />;
});

SizedImage.displayName = 'SizedImage';

export default memo(SizedImage);
