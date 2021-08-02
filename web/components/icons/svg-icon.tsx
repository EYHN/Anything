import { forwardRef } from 'react';

export type ISvgIconProps = React.SVGProps<SVGSVGElement> & { ref?: React.Ref<SVGSVGElement> };

const defaultProps = {
  viewBox: '0 0 24 24',
  color: 'inherit',
  height: '24px',
};

const SvgIcon = forwardRef<SVGSVGElement, ISvgIconProps>((props: ISvgIconProps, ref) => (
  <svg
    ref={ref}
    {...{
      ...defaultProps,
      ...props,
    }}
  />
));

SvgIcon.displayName = 'forwardRef(SvgIcon)';

export default SvgIcon;
