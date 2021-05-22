import React from 'react';
import usePixelRatio from './usePixelRatio';

interface SizedImageProps extends  React.ImgHTMLAttributes<HTMLImageElement> {
  width: number;
  height: number;
  matchlist?: Match[];
}

export interface Match {
  width: number;
  height: number;
  src: string;
}

// function parseSrcset(srcset: string) {
//   const list = srcset.split(',');
//   const result = [];
//   for (const item of list) {
//     const args = item.trim().split(/ +/g);
//     if (args.length != 2) continue;
//     const [src, size] = args;
//     const regexresult = (/(\d+)x(\d+)/g).exec(size);
//     if (regexresult?.length != 3) continue;
//     const width = parseInt(regexresult[1]);
//     const height = parseInt(regexresult[2]);
//     if (!(width && height)) continue;
//     result.push({
//       width: width,
//       height: height,
//       src: src
//     });
//   }
//   return result;
// }

function MatchSrc(matchlist: Match[], width: number, height: number) {
  matchlist.sort((a,b) => b.width * b.height - a.width * a.height);

  let result = matchlist[0].src;
  let resultpixelnum = matchlist[0].width * matchlist[0].height;

  for (const match of matchlist) {
    if (width <= match.width &&
      height <= match.height &&
      match.width * match.height < resultpixelnum) {
      result = match.src;
      resultpixelnum = match.width * match.height;
    }
  }

  return result;
}

const SizedImage = React.forwardRef<HTMLImageElement, SizedImageProps>(({width, height, matchlist, src, ...otherprops}, ref) => {
  const pixelRatio = usePixelRatio();

  const finalsrc = matchlist ? MatchSrc(matchlist, width * pixelRatio, height * pixelRatio) : src;

  return <img width={width} height={height} ref={ref} src={finalsrc} {...otherprops} />
});

SizedImage.displayName = "SizedImage";

export default React.memo(SizedImage);