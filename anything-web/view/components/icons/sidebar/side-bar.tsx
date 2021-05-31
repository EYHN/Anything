import React from 'react';
import SvgIcon, { ISvgIconProps } from 'components/icons/svg-icon';

const SideBar = (props: ISvgIconProps) => (
  <SvgIcon viewBox="0 0 24 24" {...props}>
    <path
      fillRule="evenodd"
      clipRule="evenodd"
      d="M8.88636 17V7H7V17H8.88636ZM6.5 18.5C5.94772 18.5 5.5 18.0523 5.5 17.5V6.5C5.5 5.94772 5.94772 5.5 6.5 5.5H17.5C18.0523 5.5 18.5 5.94772 18.5 6.5V17.5C18.5 18.0523 18.0523 18.5 17.5 18.5H6.5ZM10.3864 7V17H17V7H10.3864Z"
    />
  </SvgIcon>
);

export default SideBar;
