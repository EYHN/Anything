import { useEffect } from 'react';

export function useCustomEvent<T>(
  element: React.RefObject<HTMLElement> | HTMLElement | Window,
  event: string,
  callback: (this: HTMLElement, ev: CustomEvent<T>) => void,
): void {
  useEffect(() => {
    if ('current' in element) {
      element.current?.addEventListener(event, callback);
      return () => element.current?.removeEventListener(event, callback);
    } else {
      element?.addEventListener(event, callback);
      return () => element?.removeEventListener(event, callback);
    }
  }, ['current' in element ? element.current : element, event, callback]);
}

export default function useDomEvent<K extends keyof HTMLElementEventMap>(
  element: React.RefObject<HTMLElement> | HTMLElement | Window,
  event: K,
  callback: (this: HTMLElement, ev: HTMLElementEventMap[K]) => void,
): void {
  useEffect(() => {
    if ('current' in element) {
      element.current?.addEventListener(event, callback);
      return () => element.current?.removeEventListener(event, callback);
    } else {
      element?.addEventListener(event, callback);
      return () => element?.removeEventListener(event, callback);
    }
  }, ['current' in element ? element.current : element, event, callback]);
}
