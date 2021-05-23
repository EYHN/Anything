import classNames from 'classnames';
import Search from 'components/Icons/ToolBar/Search';
import React, { useCallback, useRef, useState } from 'react';
import { createUseStyles } from 'react-jss';

const useStyles = createUseStyles({
  searchbar: {
    height: '24px',
    width: '185px',
    display: 'flex',
    flexDirection: 'row',
    alignItems: 'center',
    position: 'relative',
    borderRadius: '5px',
    border: '1px solid rgba(0,0,0,0.07)',
    zIndex: 1,
    '&:before': {
      content: '""',
      display: 'inline-block',
      borderRadius: '5px',
      border: '1px solid #8BB0F5',
      width: '100%',
      height: '100%',
      position: 'absolute',
      top: 0,
      left: 0,
      opacity: 0,
      transition: '300ms opacity, 0ms transform 300ms',
      transform: 'scale(1.2, 2.5)',
      boxShadow: '0px 0px 2px #8BB0F5',
      willChange: 'opacity, transform',
      zIndex: -1,
    },
  },
  focus: {
    '&:before': {
      opacity: 1,
      transform: 'scale(1)',
      transition: '300ms opacity, 300ms transform',
    },
  },
  icon: {
    margin: '0 3px',
    height: '20px',
    width: '20px',
  },
  input: {
    border: 'none',
    background: 'transparent',
    fontSize: '12px',
    marginRight: '3px',
    outline: 'none',
    '-webkit-highlight': 'none',
    '&::placeholder': {
      color: 'rgba(0,0,0,0.25)',
    },
    '&:focus': {
      border: 'none',
    },
  },
  filling: {
    flexGrow: 1,
    overflow: 'hidden',
  },
});

const SearchBar: React.FunctionComponent = () => {
  const classes = useStyles();
  const textInputRef = useRef<HTMLInputElement>(null);

  const [focus, setFocus] = useState<boolean>(false);

  const handleClick = useCallback(() => {
    setFocus(true);
    textInputRef.current?.focus();
  }, [textInputRef]);

  const handleBlur = useCallback(() => {
    setFocus(false);
  }, [textInputRef]);

  return (
    <div className={classNames(classes.searchbar, focus && classes.focus)} onClick={handleClick}>
      <Search color="rgba(0,0,0,0.6)" className={classes.icon} />
      <input ref={textInputRef} type="text" placeholder="搜索" className={classNames(classes.input, classes.filling)} onBlur={handleBlur} />
    </div>
  );
};

export default SearchBar;
