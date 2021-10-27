import styled from '@emotion/styled';
import { Close, PlusCircle } from 'components/icons';
import { useI18n } from 'i18n';
import React, { useCallback, useMemo, useState } from 'react';
import { GroupContainer, GroupHeader } from './group';

export interface TagsGroupProps {
  className?: string;
  tags: ReadonlyArray<string>;
  onAddTag?: (tag: string) => void;
  onRemoveTag?: (tag: string) => void;
}

const Tag = styled.span(({ theme }) => ({
  position: 'relative',
  display: 'inline-block',
  backgroundColor: theme.colors.gray400,
  color: theme.colors.gray200,
  height: '20px',
  padding: '3px 12px',
  fontSize: '12px',
  lineHeight: '14px',
  borderRadius: '6px',
  overflow: 'hidden',
  '> .delete-button': {
    transition: '300ms opacity',
    opacity: 0,
  },
  '&:hover > .delete-button': {
    opacity: 1,
  },
}));

const TagsContainer = styled.div(() => ({
  display: 'flex',
  rowGap: '8px',
  columnGap: '6px',
  flexWrap: 'wrap',
}));

const AddTags = styled.span({
  display: 'inline-block',
  cursor: 'pointer',
  height: '20px',
  lineHeight: '20px',
});

const AddTagsIcon = styled(PlusCircle)(({ theme }) => ({
  color: theme.colors.gray300,
  width: '20px',
  height: '20px',
  padding: '2px',
}));

const AddTagsText = styled.span(({ theme }) => ({
  display: 'inline-block',
  padding: '3px 0',
  marginLeft: '4px',
  fontSize: '12px',
  lineHeight: '14px',
  color: theme.colors.gray300,
}));

const TagContentForm = styled.form({
  flexGrow: 1,
  minWidth: '7em',
  width: '7em',
  height: '20px',
  padding: '0',
  margin: '0',
});

const TagContentInput = styled.input(() => ({
  display: 'block',
  height: '20px',
  padding: '3px',
  fontSize: '12px',
  lineHeight: '14px',
  width: '100%',
  outline: 'none',
  border: 'none',
  WebkitTapHighlightColor: 'transparent',
  backgroundColor: 'transparent',
  '&:focus': {
    outline: 'none',
  },
}));

const DeleteTagButton = styled(Close)(({ theme }) => ({
  position: 'absolute',
  right: 0,
  top: 0,
  padding: '3px 4px 3px 6px',
  width: '14px',
  height: '14px',
  boxSizing: 'content-box',
  background: `linear-gradient(to right, rgba(0,0,0,0) 0%, ${theme.colors.gray400} 30% 100%)`,
  boxShadow: `0px 0px 16px ${theme.colors.gray400}`,
  cursor: 'pointer',
  color: theme.colors.gray300,
}));

const TagsGroup: React.VFC<TagsGroupProps> = ({ className, tags, onAddTag, onRemoveTag }) => {
  const { localeUI } = useI18n();

  const [addingNew, setAddingNew] = useState(false);
  const [contentInput, setContentInput] = useState('');

  const handleContentInputChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setContentInput(e.target.value);
  }, []);

  const handleClickAddTags = useCallback(() => {
    setAddingNew(true);
  }, []);

  const handleContentFormSubmit = useCallback(
    (e: React.FormEvent) => {
      const newTagContent = contentInput.trim();
      if (newTagContent) {
        onAddTag?.(contentInput);
      }
      setAddingNew(false);
      setContentInput('');
      e.preventDefault();
    },
    [contentInput, onAddTag],
  );

  const handleContentInputBlur = useCallback(() => {
    setAddingNew(false);
    setContentInput('');
  }, []);

  const tagElements = useMemo(() => {
    return tags.map((tag, i) => (
      <Tag key={i}>
        {tag}
        <DeleteTagButton className="delete-button" onClick={() => onRemoveTag && onRemoveTag(tag)} />
      </Tag>
    ));
  }, [tags, onRemoveTag]);

  return (
    <GroupContainer className={className}>
      <GroupHeader>{localeUI('UI.Tags')}</GroupHeader>
      <TagsContainer>
        {tagElements}
        {addingNew ? (
          <TagContentForm onSubmit={handleContentFormSubmit}>
            <TagContentInput
              type="text"
              placeholder={localeUI('UI.AddTagPlaceholder')}
              autoFocus
              onBlur={handleContentInputBlur}
              onChange={handleContentInputChange}
            />
          </TagContentForm>
        ) : (
          <AddTags onClick={handleClickAddTags}>
            <AddTagsIcon />
            {tags.length === 0 && <AddTagsText>{localeUI('UI.AddTagTip')}</AddTagsText>}
          </AddTags>
        )}
      </TagsContainer>
    </GroupContainer>
  );
};

export default TagsGroup;
