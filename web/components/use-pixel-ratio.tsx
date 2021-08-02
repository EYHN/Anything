import { useEffect, useState } from 'react';

export default function usePixelRatio(): number {
  const [pixelRatio, setPixelRatio] = useState(() => window.devicePixelRatio);
  useEffect(() => {
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
