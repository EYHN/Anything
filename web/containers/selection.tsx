import { enableMapSet, produce } from 'immer';
import React, { useContext, useReducer } from 'react';

enableMapSet();

interface SelectionState {
  selected: Set<string>;
}

const initialState = { selected: new Set<string>(), selecting: null };

type SelectionAction = { type: 'selected'; payload: string[] } | { type: 'clear' };

type SelectionContextValue = SelectionState & { dispatch: React.Dispatch<SelectionAction> };

const SelectionContext = React.createContext<SelectionContextValue | null>(null);

export function useSelection(): SelectionContextValue {
  const selection = useContext(SelectionContext);
  if (!selection) throw new Error();
  return selection;
}

const reducer = produce((state: SelectionState, action: SelectionAction) => {
  switch (action.type) {
    case 'selected':
      action.payload.forEach((item) => state.selected.add(item));
      break;
    case 'clear':
      state.selected.clear();
      break;
  }
});

export const Provider: React.FunctionComponent = ({ children }) => {
  const [selectionState, dispatch] = useReducer(reducer, initialState);

  return <SelectionContext.Provider value={{ ...selectionState, dispatch }}>{children}</SelectionContext.Provider>;
};
