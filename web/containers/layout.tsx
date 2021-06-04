import React, { useCallback, useState } from 'react';
import classnames from 'classnames';
import { createUseStyles } from 'react-jss';
import SplitView from 'components/split-view';
import useElementSize from 'components/use-element-size';

const useStyles = createUseStyles({
  container: {
    width: '100vw',
    height: '100vh',
  },
  top: {
    background: '#FFFFFF',
    height: '48px',
    borderBottom: '1px solid #dddddd',
    overflow: 'hidden',
  },
  bottom: {
    background: '#FFFFFF',
    borderTop: '1px solid #dddddd',
    height: '28px',
    overflow: 'hidden',
  },
  maincontainer: {
    display: 'block',
    flexDirection: 'column',
    height: '100%',
  },
  center: {
    background: '#FFFFFF',
    height: '100%',
  },
  right: {
    background: '#FFFFFF',
    borderLeft: '1px solid #dddddd',
    height: '100%',
    overflow: 'auto',
  },
  left: {
    background: '#EDEDED',
    borderRight: '1px solid #dddddd',
    overflow: 'hidden',
    height: '100%',
  },
  sash: {
    height: '100%',
    width: '4px',
    position: 'absolute',
    top: '0',
  },
  splitView: {
    height: '100%',
    width: '100%',
  },
});

interface IAppLayoutProps {
  top?: (width: number, height: number) => React.ReactNode;
  bottom?: (width: number, height: number) => React.ReactNode;
  left?: (width: number, height: number) => React.ReactNode;
  center?: (width: number, height: number) => React.ReactNode;
  right?: (width: number, height: number) => React.ReactNode;
}

const windowWidth = window.innerWidth;
const windowHeight = window.innerHeight;

const AppLayout: React.FunctionComponent<IAppLayoutProps> = ({ top, bottom, left, center, right }) => {
  const classes = useStyles();
  const containerRef = React.useRef<HTMLDivElement>(null);

  // Measure size
  const { width: containerWidth = windowWidth, height: containerHeight = windowHeight } = useElementSize(containerRef, []);
  const [size, setSize] = useState<number[]>([240, 440]);
  const [innerSize, setInnerSize] = useState<number[]>([220, 260]);

  const handleSplitViewSizeChange = useCallback((size: number[]) => {
    setSize(size);
  }, []);

  const handleInnerSplitViewSizeChange = useCallback((size: number[]) => {
    setInnerSize(size);
  }, []);

  return (
    <div className={classes.container} ref={containerRef}>
      {containerWidth && containerHeight && (
        <SplitView
          className={classes.splitView}
          minSize={[200, 440]}
          size={size}
          width={containerWidth}
          height={containerHeight}
          onSizeChange={handleSplitViewSizeChange}
          grow={[0, 1]}
        >
          {[
            (width, height) => (
              <div className={classes.left} key="left">
                {left && left(width, height)}
              </div>
            ),
            (width, height) => (
              <div className={classnames(classes.maincontainer)} key="maincontainer">
                <div className={classes.top}>{top && top(width, 48)}</div>
                <SplitView
                  minSize={[220, 220]}
                  size={innerSize}
                  width={width}
                  height={height - 48 - 28}
                  onSizeChange={handleInnerSplitViewSizeChange}
                  grow={[1, 0]}
                >
                  {[
                    (width, height) => <div className={classes.center}>{center && center(width, height)}</div>,
                    (width, height) => <div className={classes.right}>{right && right(width, height)}</div>,
                  ]}
                </SplitView>

                <div className={classes.bottom}>{bottom && bottom(width, 28)}</div>
              </div>
            ),
          ]}
        </SplitView>
      )}
    </div>
  );
};

export default AppLayout;
