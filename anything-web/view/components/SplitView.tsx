import React, { useCallback, useState } from 'react';
import classNames from 'classnames';
import useDomEvent from './useDomEvent';
import { createUseStyles } from 'react-jss';

interface ISplitViewProps {
  className?: string;
  children: ((width: number, height: number) => React.ReactNode)[];
  size: number[];
  onSizeChange?: (size: number[]) => void;
  width: number;
  height: number;
  grow: number[];
  minSize: number[];
}

const useStyles = createUseStyles({
  splitView: {
    position: 'relative',
    overflow: 'hidden'
  },
  sashContinter: {
    position: 'absolute',
    top: 0,
    left: 0,
    width: '100%',
    height: '100%',
    pointerEvents: 'none',
  },
  sash: {
    position: 'absolute',
    pointerEvents: 'auto',
    touchAction: 'none',
    zIndex: 1
  },
  sashVertical: {
    top: 0,
    width: 4,
    height: '100%',
    cursor: 'e-resize'
  },
  viewContinter: {
    position: 'relative',
    width: '100%',
    height: '100%'
  },
  view: {
    position: 'absolute'
  },
  viewVertical: {
    top: 0,
    height: '100%'
  }
})

const SplitView: React.FunctionComponent<ISplitViewProps> = ({ className, size, onSizeChange, children, width, height, grow, minSize }) => {
  const classes = useStyles();

  const sashs = [];
  const views = [];
  const length = size.length;

  const [activeSashIndex, setActiveSashIndex] = useState<number>();
  const [startPosition, setStartPosition] = useState<{x: number, y: number}>();
  const [startSize, setStartSize] = useState<number[]>();

  let finalSize = [...size];
  
  const minSizeTotal = minSize.reduce((pre, value) => pre + value, 0);
  const targetSizeTotal = Math.max(width, minSizeTotal);
  const sizeOffsetTotal = targetSizeTotal - size.reduce((pre, value) => pre + value, 0);
  if (sizeOffsetTotal !== 0) {
    const growTotal = grow.reduce((pre, value) => pre + value, 0);
    const growOffset = grow.map((grow) => {
      return grow / growTotal * sizeOffsetTotal;
    });
    const newSize = size.map((size, index) => {
      return Math.max(size + growOffset[index], minSize[index]);
    });
    const newSizeTotal = newSize.reduce((pre, value) => pre + value, 0);

    let remainingSize = targetSizeTotal - newSizeTotal;
    for (let i = newSize.length - 1; i >= 0; i--) {
      const tmp = Math.max(minSize[i], newSize[i] + remainingSize);
      remainingSize -= (tmp - newSize[i]);
      newSize[i] = tmp;
    }
    finalSize = newSize;
  }

  const handleSashMouseDown = useCallback((index: number, e: React.MouseEvent) => {
    setActiveSashIndex(index);
    setStartPosition({
      x: e.clientX,
      y: e.clientY
    });
    setStartSize([...finalSize]);
  }, [finalSize]);

  const handleSashMouseUp = useCallback(() => {
    if (typeof activeSashIndex !== 'undefined') setActiveSashIndex(undefined);
  }, [activeSashIndex]);
  useDomEvent(window, 'mouseup', handleSashMouseUp);

  const handleSashMouseMove = useCallback((e: MouseEvent) => {
    if (typeof activeSashIndex !== 'undefined' && startPosition && startSize) {
      let offset = e.clientX - startPosition.x;

      const newSize = [...startSize];

      if (offset > 0) {
        offset = startSize[activeSashIndex + 1] - Math.max(minSize[activeSashIndex + 1], startSize[activeSashIndex + 1] - offset);
      } else {
        offset = Math.max(minSize[activeSashIndex], startSize[activeSashIndex] + offset) - startSize[activeSashIndex];
      }
      newSize[activeSashIndex] = startSize[activeSashIndex] + offset;
      newSize[activeSashIndex + 1] = startSize[activeSashIndex + 1] - offset;

      if (typeof onSizeChange === 'function') onSizeChange(newSize);
    }
  }, [minSize, activeSashIndex, startPosition, startSize]);
  useDomEvent(window, 'mousemove', handleSashMouseMove);

  let offset = 0;
  for (let i = 0; i < length - 1; i++) {
    offset += finalSize[i];
    const pos = offset - 2;
    sashs.push(
      <div
        key={"sash-" + i}
        className={classNames(classes.sash, classes.sashVertical)}
        style={{left: pos}}
        onMouseDown={(e) => handleSashMouseDown(i, e)}
      />
    );
  }

  offset = 0;
  for (let i = 0; i < length; i++) {
    const pos = offset;
    views.push(
      <div
        key={"view-" + i}
        className={classNames(classes.view, classes.viewVertical)}
        style={{left: pos, width: finalSize[i], pointerEvents: typeof activeSashIndex !== 'undefined' ? 'none' : 'auto'}}
        draggable={false}
      >
        {typeof children[i] === 'function' ? children[i](finalSize[i], height) : children[i]}
      </div>
    );
    offset += finalSize[i];
  }

  return <div className={classNames(classes.splitView, className)} style={{width, height}}>
    {
      typeof activeSashIndex !== 'undefined' &&
      <style>
        {`* {
          cursor: e-resize !important;
          user-select: none !important;
        }`}
      </style>
    }
    <div className={classes.sashContinter}>
      {sashs}
    </div>
    <div className={classes.viewContinter}>
      {views}
    </div>
  </div>
}

export default SplitView;