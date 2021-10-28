import { MetadataSchema } from '@anything/shared';
import { Bit, ColorSpace, Edit2, Height, Rotate, Time, Width } from 'components/icons';

export const MetadataIcon: Partial<Record<keyof typeof MetadataSchema, React.ElementType<React.SVGProps<SVGSVGElement>>>> = {
  'Information.CreationTime': Time,
  'Information.LastWriteTime': Edit2,
  'Image.Width': Width,
  'Image.Height': Height,
  'Image.BitDepth': Bit,
  'Image.ColorSpace': ColorSpace,
  'Image.Orientation': Rotate,
};
