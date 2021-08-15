import useDomEvent from 'components/use-dom-event';
import { useCallback, useMemo, useRef, useState } from 'react';
import { IRect } from 'utils/rect';
import shallowEqual from 'utils/shallow-equal';

export interface FrameRect {
  x1: number;
  x2: number;
  y1: number;
  y2: number;
}

function frameToRect(frame: FrameRect): IRect {
  return {
    left: Math.min(frame.x1, frame.x2),
    top: Math.min(frame.y1, frame.y2),
    right: Math.max(frame.x1, frame.x2),
    bottom: Math.max(frame.y1, frame.y2),
  };
}

const useBoxSelect = (containerRef: React.RefObject<HTMLElement>, containerClientRect: IRect | undefined) => {
  const scrollTopRef = useRef<number>(0);
  const mousePositionRef = useRef<{ x: number; y: number }>();
  const [selecting, setSelecting] = useState(false);
  const [selectingFrame, setSelectionFrame] = useState<FrameRect>();

  const containerClientRectRef = useRef<typeof containerClientRect>();
  containerClientRectRef.current = containerClientRect;

  const handleOnScroll = useCallback(() => {
    if (!containerRef.current) return;
    scrollTopRef.current = containerRef.current.scrollTop;
  }, [containerRef]);
  useDomEvent(containerRef, 'scroll', handleOnScroll);

  const scrollAnimationFrameRef = useRef<number>(-1);
  const handleOnMouseDown = useCallback(
    (e: MouseEvent) => {
      if (e.target != containerRef.current) return;
      let prevFrameDate: number | null = null;
      let prevSelectionFrame: FrameRect | null = null;
      setSelecting(true);

      function update() {
        if (containerRef.current && containerClientRectRef.current && mousePositionRef.current) {
          const scrollTop = containerRef.current.scrollTop;
          const clientX = mousePositionRef.current.x;
          const clientY = mousePositionRef.current.y;
          const x = clientX - containerClientRectRef.current.left;
          const y = clientY - containerClientRectRef.current.top + scrollTop;
          const newSelectionFrame = { x1: x, y1: y, ...prevSelectionFrame, x2: x, y2: y };
          prevSelectionFrame = newSelectionFrame;
          setSelectionFrame((prevSelectionFrame) =>
            shallowEqual(newSelectionFrame, prevSelectionFrame) ? prevSelectionFrame : newSelectionFrame,
          );

          let offscreenY = 0;
          if (clientY > containerClientRectRef.current.bottom) {
            offscreenY = clientY - containerClientRectRef.current.bottom;
          }
          if (clientY < containerClientRectRef.current.top) {
            offscreenY = clientY - containerClientRectRef.current.top;
          }

          if (offscreenY != 0) {
            const frametime = prevFrameDate ? Date.now() - prevFrameDate : 16;
            prevFrameDate = Date.now();
            const speed = (Math.sqrt(Math.abs(offscreenY)) * 2 * frametime) / 16;
            containerRef.current.scrollBy(0, offscreenY < 0 ? -speed : speed);
          } else {
            prevFrameDate = null;
          }
        }

        cancelAnimationFrame(scrollAnimationFrameRef.current);
        scrollAnimationFrameRef.current = requestAnimationFrame(update);
      }
      update();
    },
    [containerRef],
  );
  useDomEvent(containerRef, 'mousedown', handleOnMouseDown);

  const handleOnMouseMove = useCallback(
    (e: MouseEvent) => {
      if (selecting) {
        mousePositionRef.current = { x: e.clientX, y: e.clientY };
      }
    },
    [selecting],
  );
  useDomEvent(window, 'mousemove', handleOnMouseMove);

  const Unselect = useCallback(() => {
    setSelectionFrame(undefined);
    if (scrollAnimationFrameRef.current) cancelAnimationFrame(scrollAnimationFrameRef.current);
  }, []);
  useDomEvent(window, 'blur', Unselect);
  useDomEvent(window, 'mouseup', Unselect);

  const selectingRect = useMemo(() => {
    return selectingFrame && frameToRect(selectingFrame);
  }, [selectingFrame]);

  return { selectingRect };
};

export default useBoxSelect;
