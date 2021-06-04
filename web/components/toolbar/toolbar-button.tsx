import classNames from 'classnames';
import IconButton from 'components/icons/icon-button';
import { ISvgIconProps } from 'components/icons/svg-icon';
import React from 'react';
import { createUseStyles } from 'react-jss';

interface IToolbarButtonProps {
  icon: (props: ISvgIconProps) => JSX.Element;
  className?: string;
  disabled?: boolean;
  focus?: boolean;
  active?: boolean;
  onMouseOver?: React.MouseEventHandler;
  onMouseOut?: React.MouseEventHandler;
}

const useStyles = createUseStyles({
  iconButton: {
    padding: '0 4px',
    alignItems: 'center',
    background: 'rgba(0,0,0,0)',
    borderRadius: '5px',
    '&:hover': {
      background: 'rgba(0,0,0,0.05)',
    },
    '&:active': {
      background: 'rgba(0,0,0,0.1)',
    },
  },
  disabled: {
    '&:hover': {
      background: 'rgba(0,0,0,0.0)',
    },
  },
  active: {
    background: 'rgba(0,0,0,0.05)',
  },
  focus: {
    background: 'rgba(0,0,0,0.05)',
  },
  focusActive: {
    background: 'rgba(0,0,0,0.1)',
    '&:hover': {
      background: 'rgba(0,0,0,0.1)',
    },
    '&:active': {
      background: 'rgba(0,0,0,0.15)',
    },
  },
});

const ToolBarButton: React.FunctionComponent<IToolbarButtonProps> = ({
  icon,
  className,
  disabled,
  active,
  focus,
  onMouseOver,
  onMouseOut,
}) => {
  const classes = useStyles();

  return (
    <IconButton
      disabled={disabled}
      disabledColor="rgba(0,0,0,0.25)"
      icon={icon}
      color="rgba(0,0,0,0.6)"
      className={classNames(
        classes.iconButton,
        disabled && classes.disabled,
        active && classes.active,
        focus && classes.focus,
        active && focus && classes.focusActive,
        className,
      )}
      onMouseOver={onMouseOver}
      onMouseOut={onMouseOut}
    />
  );
};

export default ToolBarButton;
