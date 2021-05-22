import React from 'react';
import SvgIcon, { ISvgIconProps } from 'components/Icons/SvgIcon';

const ListLayout = (props: ISvgIconProps) => (
  <SvgIcon viewBox='0 0 24 24' {...props}>
    <path d="M4.25 11.2604H5.75V12.7604H4.25V11.2604Z"/>
    <path d="M7.75 11.2392H19.75V12.7392H7.75V11.2392Z"/>
    <path d="M4.25 15.7711H5.75V17.2711H4.25V15.7711Z"/>
    <path d="M7.75 15.7498H19.75V17.2498H7.75V15.7498Z"/>
    <path d="M4.25 6.75018H5.75V8.25018H4.25V6.75018Z"/>
    <path d="M7.75 6.72894H19.75V8.22894H7.75V6.72894Z"/>
  </SvgIcon>
);

export default ListLayout;