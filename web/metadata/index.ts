export { MetadataIcon } from './metadata-icon';

import { MetadataSchema } from '@anything/shared';

export type MetadataPayloadKey = keyof typeof MetadataSchema | `[Advanced] ${keyof typeof MetadataSchema}`;

export type MetadataPayload = Partial<Record<MetadataPayloadKey, string | number>>;

type GetMetadataGroupKey<T, S extends string = ``> =
  | ''
  | (T extends `${infer P}.${infer E}` ? GetMetadataGroupKey<E, `${S extends `` ? `` : `${S}.`}${P}`> : S extends `` ? never : S);

export type MetadataGroupKey = GetMetadataGroupKey<keyof typeof MetadataSchema>;

export interface MetadataEntry {
  key: keyof typeof MetadataSchema;
  value: string | number;
  advanced: boolean;
}

export function parseMetadataPayload(metadataPayload: MetadataPayload) {
  const payloadKeys = Reflect.ownKeys(metadataPayload) as MetadataPayloadKey[];

  const grouped: Partial<Record<MetadataGroupKey, MetadataEntry[]>> = {};

  for (const payloadKey of payloadKeys) {
    const value = metadataPayload[payloadKey];
    if (typeof value === 'undefined') continue;
    const advanced = payloadKey.startsWith('[Advanced] ');
    const key = (advanced ? payloadKey.substring('[Advanced] '.length) : payloadKey) as keyof typeof MetadataSchema;
    const group = key.substring(0, key.lastIndexOf('.')) as MetadataGroupKey;

    const entry = {
      value,
      key,
      advanced,
    };

    if (grouped[group] instanceof Array) {
      grouped[group]?.push(entry);
    } else {
      grouped[group] = [entry];
    }
  }

  return grouped;
}
