import { forwardRef } from 'react';
import { ISvgIconProps } from './svg-icon';

export interface IIconButtonProps extends React.SVGProps<SVGSVGElement> {
  icon: (props: ISvgIconProps) => JSX.Element;
  className?: string;
  color?: string;
  size?: number;
  onClick?: React.MouseEventHandler;
  disabled?: boolean;
  disabledColor?: string;
  ref?: React.Ref<SVGSVGElement>;
}

const IconButton = forwardRef<SVGSVGElement, IIconButtonProps>(
  ({ icon: Icon, className, color = '#767676', size = 24, onClick, disabled, disabledColor = '#ccc', ...otherProps }, ref) => (
    <Icon
      color={disabled ? disabledColor : color}
      className={className}
      style={{ cursor: disabled ? 'default' : 'pointer', color: disabled ? disabledColor : color }}
      height={size}
      onClick={!disabled ? onClick : undefined}
      role="button"
      ref={ref}
      {...otherProps}
    />
  ),
);

IconButton.displayName = 'forwardRef(IconButton)';

export default IconButton;
