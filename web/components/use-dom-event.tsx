import { useEffect } from 'react';

export default function useDomEvent<K extends keyof (HTMLElementEventMap | WindowEventMap)>(
  element: React.RefObject<HTMLElement> | HTMLElement | Window,
  event: K,
  callback: (this: HTMLElement | Window, ev: (HTMLElementEventMap | WindowEventMap)[K]) => void,
): void {
  const targetElement = 'current' in element ? element.current : element;
  useEffect(() => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    targetElement?.addEventListener(event, callback as any);
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    return () => targetElement?.removeEventListener(event, callback as any);
  }, [targetElement, event, callback]);
}
