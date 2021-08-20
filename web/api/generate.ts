import fs from 'fs';
import path from 'path';
import childProcess from 'child_process';
import { generate } from '@graphql-codegen/cli';

const schemaPath = path.resolve(__dirname, './tmp/schema.graphql');
const outputPath = path.resolve(__dirname, './generated/index.ts');
const documentsPath = path.resolve(__dirname, 'documents.gql');

if (!fs.existsSync(path.join(__dirname, './tmp'))) {
  fs.mkdirSync(path.join(__dirname, './tmp'));
}

function exec(command: string) {
  console.log(`RUN: ${command}`);

  return childProcess.execSync(command, { encoding: 'utf-8', cwd: __dirname });
}

const graphqlSchemaToolProjectPath = path.resolve(__dirname, '../../core/Anything.Tools.GraphqlSchema/Anything.Tools.GraphqlSchema.csproj');

exec(`dotnet build "${graphqlSchemaToolProjectPath}"`);

const schemaString = exec(`dotnet run --no-build --no-restore --no-launch-profile --project "${graphqlSchemaToolProjectPath}"`);

fs.writeFileSync(schemaPath, schemaString, 'utf-8');

generate({
  hooks: {
    afterOneFileWrite: ['prettier --write'],
  },
  overwrite: true,
  schema: schemaPath,
  documents: documentsPath,
  generates: {
    [outputPath]: {
      plugins: [
        { add: { content: '/* eslint-disable */' } },
        'typescript',
        'typescript-operations',
        'typescript-react-apollo',
        'fragment-matcher',
      ],
      config: {
        strict: true,
        immutableTypes: true,
        typesPrefix: 'I',
        preResolveTypes: true,
        onlyOperationTypes: true,
        scalars: {
          Url: 'string',
          DateTimeOffset: 'string',
        },
      },
    },
  },
});
