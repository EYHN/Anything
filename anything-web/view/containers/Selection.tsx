import React, { useContext, useState } from 'react';

type FilePath = string;
type Selection = FilePath[];
type SelectionContextValue = [Selection, React.Dispatch<React.SetStateAction<Selection>>];

const SelectionContext = React.createContext<SelectionContextValue>(null!);

export function useSelection() {
  return useContext(SelectionContext);
}

export const Provider: React.FunctionComponent = ({children}) => {
  const [selection, setSelection] = useState<Selection>([]);

  return <SelectionContext.Provider value={[selection, setSelection]}>
    {children}
  </SelectionContext.Provider>
}