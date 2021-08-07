import { forwardRef, memo } from 'react';
import usePixelRatio from './use-pixel-ratio';

interface SizedImageProps extends React.ImgHTMLAttributes<HTMLImageElement> {
  width: number;
  height: number;
  matchlist?: Match[];
}

export interface Match {
  width: number;
  height: number;
  src: string;
}

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

  return <img width={width} height={height} ref={ref} src={finalsrc} {...otherprops} />;
});

SizedImage.displayName = 'SizedImage';

export default memo(SizedImage);
