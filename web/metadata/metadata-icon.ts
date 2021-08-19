import { MetadataSchema } from '@anything/shared';
import { Bit, ColorSpace, Edit, Height, Rotate, Time, Width } from 'components/icons';

export const MetadataIcon: Partial<Record<keyof typeof MetadataSchema, React.ElementType<React.SVGProps<SVGSVGElement>>>> = {
  'Information.CreationTime': Time,
  'Information.LastWriteTime': Edit,
  'Image.Width': Width,
  'Image.Height': Height,
  'Image.BitDepth': Bit,
  'Image.ColorSpace': ColorSpace,
  'Image.Orientation': Rotate,
};
