
type RawMetadata = Record<string, any>;

type MetadataItem = {
  key: string,
  fullKey: string,
  value: string | number,
  attributes: string[]
}

export interface MetadataDictionary {
  [category: string]: MetadataItem[]
}

export function parseMetadataDictionary(data: RawMetadata) {
  const dictionary: MetadataDictionary = {};

  Object.keys(data).forEach((rawKey) => {
    const regexResult = /^(?<attributes>(\s*\[.+\]\s*)*)(?<fullKey>((?<category>\w+(\.\w+)*)\.)?(?<key>\w+))$/.exec(rawKey)
    const attributes = Array.from(regexResult?.groups?.attributes?.matchAll(/\[(?<attribute>[^\]]+)\]/g) || []).map((match) => match.groups!.attribute);

    const fullKey = regexResult?.groups?.fullKey || rawKey;
    const key = regexResult?.groups?.key || rawKey;
    const category = regexResult?.groups?.category || key;
    const value = data[rawKey];

    if (!(category in dictionary)) {
      dictionary[category] = [];
    }
    
    dictionary[category].push({
      key,
      fullKey,
      value,
      attributes
    });
  })

  return dictionary;
}