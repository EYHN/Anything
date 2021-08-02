import SvgIcon, { ISvgIconProps } from 'components/icons/svg-icon';

const Menu = (props: ISvgIconProps) => (
  <SvgIcon viewBox="0 0 24 24" {...props}>
    <path fillRule="evenodd" clipRule="evenodd" d="M19 8.75H5V7.25H19V8.75ZM19 12.75H5V11.25H19V12.75ZM19 16.75H5V15.25H19V16.75Z" />
  </SvgIcon>
);

export default Menu;
