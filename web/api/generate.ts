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

const projectPath = path.resolve(__dirname, '../../core/Anything/Anything.csproj');

exec(`dotnet build "${projectPath}"`);

const schemaString = exec(
  `dotnet run --no-build --no-restore --no-launch-profile --project "${projectPath}" -- server --print-graphql-schema`,
);

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
          FileHandle: '{ identifier: string }',
        },
      },
    },
  },
}).catch((err) => console.log(err.errors));
