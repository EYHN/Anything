import SvgIcon, { ISvgIconProps } from 'components/icons/svg-icon';

const Add = (props: ISvgIconProps) => (
  <SvgIcon viewBox="0 0 24 24" {...props}>
    <path fillRule="evenodd" clipRule="evenodd" d="M11.25 11.25V5.5H12.75V11.25H18.5V12.75H12.75V18.5H11.25V12.75H5.5V11.25H11.25Z" />
  </SvgIcon>
);

export default Add;
