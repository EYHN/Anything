import metadataSchema from './schema.json';
import fs from 'fs';
import path from 'path';

type MetadataSchema = typeof metadataSchema;
type MetadataIds = keyof MetadataSchema;
type MetadataValueSchema = MetadataSchema[keyof MetadataSchema];

const noModifyAlert = `/* This file is automatically generated by @anything/shared, please do not modify this file. */`;

function generateCSharp() {
  type NestedValueType = { type: 'Nested'; name: string };
  const namespace = `Anything.Preview.Meta.Schema`;
  const classnameSuffix = `Metadata`;

  const classes: { [classname: string]: { [valuename: string]: MetadataValueSchema | NestedValueType } } = { '': {} };

  for (const metadataId in metadataSchema) {
    const metadataValue = metadataSchema[metadataId as MetadataIds];
    const parts = metadataId.split('.');
    const classnameParts = parts.splice(0, parts.length - 1);
    const valuename = parts.join('');
    const current: string[] = [];
    if (classnameParts.length > 0) {
      for (const part of classnameParts) {
        const parent = current.join('');
        current.push(part);
        const classname = current.join('');
        if (!(classname in classes)) {
          classes[parent][classname] = { type: 'Nested', name: classname };
          classes[classname] = { [valuename]: metadataValue };
        }
      }
      classes[classnameParts.join('')][valuename] = metadataValue;
    } else {
      classes[''][valuename] = metadataValue;
    }
  }

  function convertValueType(value: MetadataValueSchema) {
    switch (value.type) {
      case 'String':
        return 'string?';
      case 'Int':
        return 'int?';
      case 'Float':
        return 'double?';
      case 'DateTime':
        return 'System.DateTimeOffset?';
      case 'TimeSpan':
        return 'System.TimeSpan?';
      default:
        throw new Error('Type not support: ' + value.type);
    }
  }

  function GetAttributes(value: MetadataValueSchema) {
    const attributes = [];
    if ('advanced' in value && value.advanced) {
      attributes.push('MetadataAdvanced');
    }

    return attributes;
  }

  return `${noModifyAlert}
#pragma warning disable
namespace ${namespace}
{
${Object.keys(classes)
  .map(
    (classname) => `    public partial class ${classname + classnameSuffix} : IMetadata
    {
${Object.keys(classes[classname])
  .map((valuename) => {
    const value = classes[classname][valuename];
    if (value.type === 'Nested') {
      return `        public ${(value as NestedValueType).name + classnameSuffix} ${valuename} = new();`;
    } else {
      return `        ${GetAttributes(value).map((attr) => `[${attr}]\n        `)}public ${convertValueType(value)} ${valuename};`;
    }
  })
  .join('\n\n')}
    }`,
  )
  .join('\n\n')}
}`;
}

function generateTypescript() {
  return `${noModifyAlert}
/* eslint-disable */
export const MetadataSchema = ${JSON.stringify(metadataSchema, undefined, 2)} as const;
`;
}

if (!fs.existsSync(path.join(__dirname, './generated'))) {
  fs.mkdirSync(path.join(__dirname, './generated'));
}

fs.writeFileSync(path.join(__dirname, './generated/schema.cs'), generateCSharp());
fs.writeFileSync(path.join(__dirname, './generated/schema.ts'), generateTypescript());
