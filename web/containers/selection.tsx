import { enableMapSet, produce } from 'immer';
import React, { useContext, useReducer } from 'react';

enableMapSet();

interface SelectionState {
  selected: Set<string>;
}

const initialState = { selected: new Set<string>(), selecting: null };

export enum SelectionActionType {
  Single = 1,
  ShiftSingle = 2,
  Multiple = 3,
  Clear = 100,
}

type SelectionAction =
  | { type: SelectionActionType.Single; payload: string }
  | { type: SelectionActionType.ShiftSingle; payload: string }
  | { type: SelectionActionType.Multiple; payload: string[] }
  | { type: SelectionActionType.Clear };

type SelectionContextValue = SelectionState & { dispatch: React.Dispatch<SelectionAction> };

const SelectionContext = React.createContext<SelectionContextValue | null>(null);

export function useSelection(): SelectionContextValue {
  const selection = useContext(SelectionContext);
  if (!selection) throw new Error();
  return selection;
}

const reducer = produce((state: SelectionState, action: SelectionAction) => {
  switch (action.type) {
    case SelectionActionType.Single:
      state.selected = new Set([action.payload]);
      break;
    case SelectionActionType.ShiftSingle:
      state.selected.has(action.payload) ? state.selected.delete(action.payload) : state.selected.add(action.payload);
      break;
    case SelectionActionType.Multiple:
      action.payload.forEach((item) => state.selected.add(item));
      break;
    case SelectionActionType.Clear:
      state.selected.clear();
      break;
  }
});

export const Provider: React.FunctionComponent = ({ children }) => {
  const [selectionState, dispatch] = useReducer(reducer, initialState);

  return <SelectionContext.Provider value={{ ...selectionState, dispatch }}>{children}</SelectionContext.Provider>;
};
