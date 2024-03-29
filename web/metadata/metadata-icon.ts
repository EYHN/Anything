import { MetadataSchema } from '@anything/shared';
import {
  Bit,
  ColorSpace,
  Edit2,
  Height,
  Rotate,
  NewlyBuild,
  Width,
  Time,
  Record,
  Entertainment,
  MusicRhythm,
  RecordDisc,
  MusicList,
  Music,
  Piano,
  People,
} from 'components/icons';

export const MetadataIcon: Partial<Record<keyof typeof MetadataSchema, React.ElementType<React.SVGProps<SVGSVGElement>>>> = {
  'Information.Duration': Time,
  'Information.CreationTime': NewlyBuild,
  'Information.LastWriteTime': Edit2,
  'Image.Width': Width,
  'Image.Height': Height,
  'Image.BitDepth': Bit,
  'Image.ColorSpace': ColorSpace,
  'Image.Orientation': Rotate,
  'Music.Title': Music,
  'Music.Artist': Entertainment,
  'Music.Album': Record,
  'Music.Genre': MusicRhythm,
  'Music.Date': Time,
  'Music.Track': MusicList,
  'Music.Disc': RecordDisc,
  'Music.Composer': Piano,
  'Music.AlbumArtist': People,
};
