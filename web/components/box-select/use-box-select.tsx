import { useEffect, useMemo, useState } from 'react';
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

const useBoxSelect = (
  containerRef: React.RefObject<HTMLElement>,
  containerClientRect: IRect | undefined,
  onSelectStart?: () => void,
  onSelectEnd?: (selectingRect: IRect) => void,
  onSelectCancel?: () => void,
) => {
  const [selectingFrame, setSelectionFrame] = useState<FrameRect>();

  const selectingRect = useMemo(() => {
    return selectingFrame && frameToRect(selectingFrame);
  }, [selectingFrame]);

  useEffect(() => {
    if (!containerRef.current || !containerClientRect) return;

    const containerRect = containerClientRect;

    const containerElement = containerRef.current;

    let scrollTop = containerElement.scrollTop;
    let animationFrame = -1;
    let mousePosition: { x: number; y: number } | null = null;
    let selecting = false;
    let lastSelectionFrame: FrameRect | null = null;
    let lastUpdateTime: number | null = null;

    const update = (time: number) => {
      const frametime = lastUpdateTime ? time - lastUpdateTime : 16;
      lastUpdateTime = time;

      if (!mousePosition) return;
      const { x: clientX, y: clientY } = mousePosition;
      const x = clientX - containerRect.left;
      const y = clientY - containerRect.top + scrollTop;
      const newSelectionFrame = { x1: x, y1: y, ...lastSelectionFrame, x2: x, y2: y };
      lastSelectionFrame = newSelectionFrame;

      setSelectionFrame((prevSelectionFrame) =>
        shallowEqual(newSelectionFrame, prevSelectionFrame) ? prevSelectionFrame : newSelectionFrame,
      );

      let offscreenY = 0;
      if (clientY > containerRect.bottom) {
        offscreenY = clientY - containerRect.bottom;
      }
      if (clientY < containerRect.top) {
        offscreenY = clientY - containerRect.top;
      }

      if (offscreenY != 0) {
        const speed = (Math.sqrt(Math.abs(offscreenY)) * 2 * frametime) / 16;
        containerElement.scrollBy(0, offscreenY < 0 ? -speed : speed);
      } else {
        lastUpdateTime = null;
      }
      requestUpdate();
    };

    const requestUpdate = () => {
      cancelAnimationFrame(animationFrame);
      animationFrame = requestAnimationFrame(update);
    };

    const handleScroll = () => {
      scrollTop = containerElement.scrollTop;
    };

    const handleMouseDown = (e: MouseEvent) => {
      if (selecting) return;
      selecting = true;

      typeof onSelectStart === 'function' && onSelectStart();

      mousePosition = { x: e.clientX, y: e.clientY };
      requestUpdate();
    };

    const handleMouseMove = (e: MouseEvent) => {
      if (selecting) {
        mousePosition = { x: e.clientX, y: e.clientY };
      }
    };

    const endSelect = () => {
      if (selecting) {
        if (lastSelectionFrame) {
          typeof onSelectEnd === 'function' && onSelectEnd(frameToRect(lastSelectionFrame));
        } else {
          typeof onSelectCancel === 'function' && onSelectCancel();
        }
        cancelAnimationFrame(animationFrame);
        setSelectionFrame(undefined);
        selecting = false;
        lastSelectionFrame = null;
        lastUpdateTime = null;
      }
    };

    const cancelSelect = () => {
      if (selecting) {
        typeof onSelectCancel === 'function' && onSelectCancel();
        cancelAnimationFrame(animationFrame);
        setSelectionFrame(undefined);
        selecting = false;
        lastSelectionFrame = null;
        lastUpdateTime = null;
      }
    };

    containerElement.addEventListener('scroll', handleScroll);
    containerElement.addEventListener('mousedown', handleMouseDown);
    window.addEventListener('mouseup', endSelect);
    window.addEventListener('blur', cancelSelect);
    window.addEventListener('mousemove', handleMouseMove);

    return () => {
      containerElement.removeEventListener('scroll', handleScroll);
      containerElement.removeEventListener('mousedown', handleMouseDown);
      window.removeEventListener('mouseup', endSelect);
      window.removeEventListener('blur', cancelSelect);
      window.removeEventListener('mousemove', handleMouseMove);

      cancelAnimationFrame(animationFrame);

      cancelSelect();
    };
  }, [containerClientRect, containerRef, onSelectCancel, onSelectEnd, onSelectStart]);

  return { selectingRect };
};

export default useBoxSelect;
