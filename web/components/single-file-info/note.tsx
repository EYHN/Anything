import styled from '@emotion/styled';
import { Edit } from 'components/icons';
import { useI18n } from 'i18n';
import React, { useCallback, useState } from 'react';
import Action from './action';
import { GroupAction, GroupContainer, GroupHeader } from './group';

export interface NoteProps {
  className?: string;
  note: string;
  onChange?: (newNote: string) => void;
}

const Content = styled.p(({ theme }) => ({
  width: '100%',
  margin: '0',
  fontSize: '12px',
  color: theme.colors.gray200,
  lineHeight: '20px',
}));

const TextArea = styled.textarea(({ theme }) => ({
  fontFamily: "'Inter', sans-serif",
  width: '100%',
  margin: '0',
  padding: '6px',
  fontSize: '12px',
  outline: 'none',
  backgroundColor: 'transparent',
  border: `1px solid ${theme.colors.gray400}`,
  borderRadius: '6px',
  resize: 'none',
  color: theme.colors.gray100,
  '&:focus': {
    outline: 'none',
  },
}));

const Note: React.VFC<NoteProps> = ({ className, note, onChange }) => {
  const { localeUI } = useI18n();

  const [textAreaValue, setTextAreaValue] = useState('');
  const [editing, setEditing] = useState(false);

  const handleTextAreaValueChange = useCallback((e: React.ChangeEvent<HTMLTextAreaElement>) => {
    setTextAreaValue(e.target.value);
  }, []);

  const handleStartEditing = useCallback(() => {
    setTextAreaValue(note || '');
    setEditing(true);
  }, [note]);

  const handleSave = useCallback(() => {
    onChange?.(textAreaValue);
    setEditing(false);
  }, [onChange, textAreaValue]);

  return (
    <GroupContainer className={className}>
      {(note || editing) && (
        <GroupAction onClick={editing ? handleSave : handleStartEditing}>
          {editing ? localeUI('UI.SaveNote') : localeUI('UI.EditNote')}
        </GroupAction>
      )}
      <GroupHeader>{localeUI('UI.Note')}</GroupHeader>
      <div>
        {editing ? (
          <TextArea value={textAreaValue} onChange={handleTextAreaValueChange} autoFocus />
        ) : note ? (
          <Content>{note}</Content>
        ) : (
          <Action icon={Edit} label={localeUI('UI.AddNoteTip')} onClick={handleStartEditing} />
        )}
      </div>
    </GroupContainer>
  );
};

export default Note;
