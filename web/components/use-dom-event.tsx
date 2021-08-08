import { useEffect } from 'react';

export default function useDomEvent<K extends keyof HTMLElementEventMap>(
  element: React.RefObject<HTMLElement> | HTMLElement | Window,
  event: K,
  callback: (this: HTMLElement, ev: HTMLElementEventMap[K]) => void,
): void {
  const targetElement = 'current' in element ? element.current : element;
  useEffect(() => {
    targetElement?.addEventListener(event, callback);
    return () => targetElement?.removeEventListener(event, callback);
  }, [targetElement, event, callback]);
}
