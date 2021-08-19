import { IFileFragment } from 'api';
import { GridLayout } from './grid';
import { LayoutContainer } from './layout';

import { SelectionActionType, useSelection } from 'containers/selection';
import { useCallback } from 'react';

interface Props {
  className?: string;
  files: ReadonlyArray<IFileFragment>;
}

const Explorer: React.FC<Props> = ({ className, files }) => {
  const { selected, dispatch } = useSelection();

  const isSelected = useCallback(
    (file: IFileFragment) => {
      return selected.has(file.url);
    },
    [selected],
  );

  const onSelectStart = useCallback(
    (e: GeneralMouseEvent) => {
      if (!e.ctrlKey && !e.shiftKey) {
        return dispatch({ type: SelectionActionType.Clear });
      }
    },
    [dispatch],
  );

  const onSelectEnd = useCallback(
    (selected: IFileFragment[]) => {
      return dispatch({ type: SelectionActionType.Multiple, payload: selected.map((item) => item.url) });
    },
    [dispatch],
  );

  const onMouseDownItem = useCallback(
    (item: IFileFragment, e: GeneralMouseEvent) => {
      if (e.ctrlKey || e.shiftKey) {
        return dispatch({ type: SelectionActionType.ShiftSingle, payload: item.url });
      } else {
        return dispatch({ type: SelectionActionType.Single, payload: item.url });
      }
    },
    [dispatch],
  );

  return (
    <LayoutContainer
      className={className}
      data={files}
      layout={GridLayout}
      isSelected={isSelected}
      onSelectStart={onSelectStart}
      onSelectEnd={onSelectEnd}
      onMouseDownItem={onMouseDownItem}
    />
  );
};

export default Explorer;
