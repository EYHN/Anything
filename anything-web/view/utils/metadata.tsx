type RawMetadata = Record<string, string | number>;

type MetadataItem = {
  key: string;
  fullKey: string;
  value: string | number;
  attributes: string[];
};

export interface MetadataDictionary {
  [category: string]: MetadataItem[];
}

export function parseMetadataDictionary(data: RawMetadata) {
  const dictionary: MetadataDictionary = {};

  Object.keys(data).forEach((rawKey) => {
    const match = rawKey.match(/^(?<attributes>(\s*\[.+\]\s*)*)(?<fullKey>((?<category>\w+(\.\w+)*)\.)?(?<key>\w+))$/);
    if (!match) {
      return;
    }

    const attributes = Array.from(match.groups?.attributes.matchAll(/\[(?<attribute>[^\]]+)\]/g) || [])
      .map((attributeMatch) => attributeMatch.groups?.attribute)
      .filter((attribute) => !!attribute) as string[];

    const fullKey = match?.groups?.fullKey || rawKey;
    const key = match?.groups?.key || rawKey;
    const category = match?.groups?.category || key;
    const value = data[rawKey];

    if (!(category in dictionary)) {
      dictionary[category] = [];
    }

    dictionary[category].push({
      key,
      fullKey,
      value,
      attributes,
    });
  });

  return dictionary;
}
