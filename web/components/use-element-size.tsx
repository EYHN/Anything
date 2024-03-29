import isEqual from 'lodash-es/isEqual';
import { useEffect, useRef, useState } from 'react';

interface ElementSize {
  width?: number;
  height?: number;
  clientRect?: ClientRect;
}

/**
 * A hook to get the size of an element and its client rect.
 *
 * @argument elementRef The RefObject of target element.
 *
 * @example
 * const elementRef = React.useRef(null);
 * const {width=100, height=100, clientRect} = useSize(elementRef)
 *
 */
export default function useElementSize(elementRef: React.RefObject<HTMLElement>): ElementSize {
  const [clientRect, setClientRect] = useState<ClientRect>();
  const clientRectRef = useRef<ClientRect>();
  useEffect(() => {
    function update() {
      if (elementRef.current) {
        const c = elementRef.current.getClientRects()[0];
        const newClientRect = {
          bottom: c.bottom,
          height: c.height,
          left: c.left,
          right: c.right,
          top: c.top,
          width: c.width,
        };
        if (!isEqual(newClientRect, clientRectRef.current)) {
          setClientRect(newClientRect);
          clientRectRef.current = newClientRect;
        }
      }
    }
    window.addEventListener('resize', update);
    update();
    return () => window.removeEventListener('resize', update);
  }, [elementRef]);

  return { width: clientRect?.width, height: clientRect?.height, clientRect };
}
