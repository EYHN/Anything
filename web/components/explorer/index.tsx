import { IDirentFragment } from 'api';
import { GridLayout } from './grid';
import { LayoutContainer } from './layout';

import { SelectionActionType, useSelection } from 'containers/selection';
import { useCallback } from 'react';

interface Props {
  className?: string;
  files: ReadonlyArray<IDirentFragment>;
}

const Explorer: React.FC<Props> = ({ className, files }) => {
  const { selected, dispatch } = useSelection();

  const isSelected = useCallback(
    (dirent: IDirentFragment) => {
      return selected.has(dirent.file.fileHandle.value.identifier);
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
    (selected: ReadonlyArray<IDirentFragment>) => {
      return dispatch({ type: SelectionActionType.Multiple, payload: selected.map((item) => item.file.fileHandle.value.identifier) });
    },
    [dispatch],
  );

  const onMouseDownItem = useCallback(
    (item: IDirentFragment, e: GeneralMouseEvent) => {
      if (e.ctrlKey || e.shiftKey) {
        return dispatch({ type: SelectionActionType.ShiftSingle, payload: item.file.fileHandle.value.identifier });
      } else {
        return dispatch({ type: SelectionActionType.Single, payload: item.file.fileHandle.value.identifier });
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
