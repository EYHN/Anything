declare module '*.svg' {
  export const ReactComponent: import('react').ComponentClass<import('react').SVGProps<SVGSVGElement>>;
  export default ReactComponent;
}
