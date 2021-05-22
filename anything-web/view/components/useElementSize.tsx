import React from 'react';
import isEqual from 'lodash-es/isEqual';

/**
 * A hook to get the size of an element and its client rect.
 * 
 * @argument elementRef The RefObject of target element.
 * @argument deps When the value in the list changes, the size is recalculated
 * 
 * @example
 * const elementRef = React.useRef(null);
 * const {width=100, height=100, clientRect} = useSize(elementRef)
 * 
 */
export default function useElementSize(elementRef: React.RefObject<HTMLElement>, deps?: React.DependencyList) {
  const [clientRect, setClientRect] = React.useState<ClientRect | null>(null);
  const clientRectRef = React.useRef<ClientRect | null>(null);
  React.useEffect(() => {
    function update() {
      if (elementRef.current) {
        const c = elementRef.current.getClientRects()[0];
        const newClientRect = {
          bottom: c.bottom,
          height: c.height,
          left: c.left,
          right: c.right,
          top: c.top,
          width: c.width
        };
        if (!isEqual(newClientRect, clientRectRef.current)) {
          setClientRect(newClientRect);
          clientRectRef.current = newClientRect;
        }
      }
    }
    window.addEventListener('resize', update);
    update();
    return () => window.removeEventListener('resize', update)
  }, deps);

  return {width: clientRect?.width, height: clientRect?.height, clientRect};
}