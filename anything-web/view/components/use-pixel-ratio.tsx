import React from 'react';

export default function usePixelRatio(): number {
  const [pixelRatio, setPixelRatio] = React.useState(() => window.devicePixelRatio);
  React.useEffect(() => {
    const mqString = `(resolution: ${pixelRatio}dppx)`;
    const mq = matchMedia(mqString);

    const updatePixelRatio = () => {
      const pr = window.devicePixelRatio;
      setPixelRatio(pr);
    };

    mq.addEventListener('change', updatePixelRatio);

    return () => mq.removeEventListener('change', updatePixelRatio);
  }, [pixelRatio]);

  return pixelRatio;
}
