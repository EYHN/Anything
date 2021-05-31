import React from 'react';
import useDomEvent from './use-dom-event';
import useElementSize from './use-element-size';

interface BoxSelectContainerProps extends React.HTMLProps<HTMLDivElement> {
  onSelectRect?: (rect: FrameRect | null) => void;
  onSelectStart?: () => void;
  onSelectEnd?: () => void;
}

export interface FrameRect {
  x1: number;
  x2: number;
  y1: number;
  y2: number;
}

function calcFrameStyle(frame: FrameRect, scrollTop: number) {
  return {
    left: Math.min(frame.x1, frame.x2),
    top: Math.min(frame.y1, frame.y2) - scrollTop,
    width: Math.abs(frame.x1 - frame.x2),
    height: Math.abs(frame.y1 - frame.y2),
  };
}

export const BoxSelectContainer = React.forwardRef<HTMLDivElement, BoxSelectContainerProps>(
  ({ children, style, onMouseMove, onScroll, onSelectStart, onSelectRect, onSelectEnd, ...otherProps }, ref) => {
    const rootRef = React.useRef<HTMLDivElement>(null);
    React.useImperativeHandle(ref, () => rootRef.current as HTMLDivElement);

    const [mouseSelecting, setMouseSelecting] = React.useState<boolean>(false);
    const [selectionFrame, setSelectionFrame] = React.useState<FrameRect | null>(null);
    const { clientRect } = useElementSize(rootRef);

    const scrollTopRef = React.useRef<number>(0);
    const handleOnScroll = React.useCallback(
      (e: React.UIEvent<HTMLDivElement>) => {
        if (typeof onScroll === 'function') onScroll(e);
        scrollTopRef.current = e.currentTarget.scrollTop;
      },
      [scrollTopRef],
    );

    const handleOnMouseDown = React.useCallback(
      (e: React.MouseEvent<HTMLDivElement>) => {
        if (typeof onMouseMove === 'function') onMouseMove(e);
        if (!(rootRef.current && clientRect)) return;
        setMouseSelecting(true);
        const scrollTop = rootRef.current.scrollTop;
        const x = e.clientX - clientRect.left;
        const y = e.clientY - clientRect.top + scrollTop;
        setSelectionFrame({ x1: x, x2: x, y1: y, y2: y });
        typeof onSelectStart === 'function' && onSelectStart();
        rootRef.current.dispatchEvent(new CustomEvent('box-select-start'));
      },
      [clientRect, onSelectStart],
    );

    const scrollAnimationFrameRef = React.useRef<number | null>(null);
    const prevFrameDateRef = React.useRef<number | null>(null);
    const handleOnMouseMove = React.useCallback(
      (e: MouseEvent) => {
        function update() {
          if (!(clientRect && rootRef.current)) return;
          const scrollTop = rootRef.current.scrollTop;
          const x = e.clientX - clientRect.left;
          const y = e.clientY - clientRect.top + scrollTop;
          const newSelectionFrame = { x1: x, y1: y, ...selectionFrame, x2: x, y2: y };
          setSelectionFrame(newSelectionFrame);
          typeof onSelectRect === 'function' && onSelectRect(newSelectionFrame);
          rootRef.current.dispatchEvent(new CustomEvent('box-select-update', { detail: newSelectionFrame }));

          let offscreenY = 0;
          if (e.clientY > clientRect.top + clientRect.height) {
            offscreenY = e.clientY - (clientRect.top + clientRect.height);
          }
          if (e.clientY < clientRect.top) {
            offscreenY = e.clientY - clientRect.top;
          }

          if (offscreenY != 0) {
            const frametime = prevFrameDateRef.current ? Date.now() - prevFrameDateRef.current : 16;
            prevFrameDateRef.current = Date.now();
            const speed = (Math.sqrt(Math.abs(offscreenY)) * 2 * frametime) / 16;
            rootRef.current.scrollBy(0, offscreenY < 0 ? -speed : speed);
            scrollAnimationFrameRef.current = requestAnimationFrame(update);
          } else {
            prevFrameDateRef.current = null;
          }
        }
        if (mouseSelecting) {
          if (scrollAnimationFrameRef.current) cancelAnimationFrame(scrollAnimationFrameRef.current);
          update();
        }
      },
      [selectionFrame, mouseSelecting, clientRect, onSelectRect],
    );
    useDomEvent(window, 'mousemove', handleOnMouseMove);

    const Unselect = React.useCallback(() => {
      setMouseSelecting(false);
      typeof onSelectEnd === 'function' && onSelectEnd();
      rootRef.current?.dispatchEvent(new CustomEvent('box-select-end'));
      prevFrameDateRef.current = 0;
      if (scrollAnimationFrameRef.current) cancelAnimationFrame(scrollAnimationFrameRef.current);
    }, [onSelectEnd]);
    useDomEvent(window, 'blur', Unselect);
    useDomEvent(window, 'mouseup', Unselect);

    return (
      <>
        <div style={style} onMouseDown={handleOnMouseDown} onScroll={handleOnScroll} ref={rootRef} {...otherProps}>
          {children}
        </div>
        <div
          style={{
            position: 'fixed',
            width: clientRect?.width,
            height: clientRect?.height,
            top: clientRect?.top,
            left: clientRect?.left,
            overflow: 'hidden',
            pointerEvents: 'none',
          }}
        >
          {mouseSelecting && selectionFrame && (
            <div
              style={{
                position: 'absolute',
                background: 'rgba(0, 0, 0, 0.1)',
                border: '1px solid rgba(0, 0, 0, 0.32)',
                ...calcFrameStyle(selectionFrame, scrollTopRef.current),
              }}
            />
          )}
        </div>
      </>
    );
  },
);

BoxSelectContainer.displayName = 'BoxSelectContainer';

const useBoxSelectContainer = (addprops: {
  onSelectRect?: BoxSelectContainerProps['onSelectRect'];
  onSelectStart?: BoxSelectContainerProps['onSelectStart'];
  onSelectEnd?: BoxSelectContainerProps['onSelectEnd'];
}) => {
  const component = React.useMemo(() => {
    const c = React.forwardRef<HTMLDivElement, BoxSelectContainerProps>((props, ref) => {
      return <BoxSelectContainer {...props} {...addprops} ref={ref} />;
    });
    c.displayName = 'forwardRef(BoxSelectContainer)';

    return c;
  }, [addprops.onSelectRect, addprops.onSelectStart, addprops.onSelectEnd]);

  return [component];
};

export default useBoxSelectContainer;
