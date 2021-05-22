import React from 'react';

export default function usePixelRatio() {
  const [pixelRatio, setPixelRatio] = React.useState(() => window.devicePixelRatio);
  React.useEffect(() => {
    const mqString = `(resolution: ${pixelRatio}dppx)`;
    let mq = matchMedia(mqString);

    const updatePixelRatio = () => {
      let pr = window.devicePixelRatio;
      setPixelRatio(pr);
    }

    mq.addListener(updatePixelRatio)

    return () => mq.removeListener(updatePixelRatio);
  }, [pixelRatio]);

  return pixelRatio;
}