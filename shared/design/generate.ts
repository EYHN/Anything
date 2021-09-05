import got from 'got';
import fs from 'fs';
import path from 'path';
import { mkdirpSync } from 'fs-extra';
import _ from 'lodash';

const ACCESS_TOKEN = process.env.FIGMA_ACCESS_TOKEN;

const options = {
  file: 'j6B64U4ebq0nJsp6RDXfN2',
};

interface CanvasNode extends BaseNodeMixin, ChildrenMixin {
  readonly type: 'CANVAS';
}

type FigmaNode = SceneNode | BaseNode | CanvasNode;

type NodeType = FigmaNode['type'];

const httpApi = got.extend({
  hooks: {
    beforeRequest: [
      (options) => {
        console.log(`[${options.method}] ${options.url.toString()}`);
      },
    ],
    beforeError: [
      (error) => {
        const { response } = error;
        if (response && response.body) {
          error.name = 'Figma Error';
          error.message = `${JSON.stringify(response.body)} (${response.statusCode})`;
        }

        return error;
      },
    ],
    beforeRetry: [
      (_, error, retryCount) => {
        console.log(`Retrying [${retryCount}]: ${error?.code}`);
      },
    ],
  },
  retry: 3,
});

const figmaApi = got.extend(httpApi, {
  prefixUrl: 'https://api.figma.com/v1',
  headers: {
    'X-Figma-Token': ACCESS_TOKEN,
  },
});

function getFile(fileKey: string) {
  return figmaApi(`files/${fileKey}`).json<{ document: DocumentNode }>();
}

async function getExport(fileKey: string, nodeId: string, exportSettings: ExportSettings) {
  let scale = 1;
  if (exportSettings.format === 'JPG' || exportSettings.format === 'PNG') {
    if (exportSettings.constraint?.type === 'SCALE') {
      scale = exportSettings.constraint.value;
    } else {
      throw new Error('not support');
    }
  }
  const data = await figmaApi(`images/${fileKey}`, {
    searchParams: { ids: nodeId, format: exportSettings.format.toLowerCase(), scale: scale },
  }).json<{
    err: string;
    images: { [key: string]: string };
  }>();

  if (data.err) {
    throw new Error(data.err);
  }

  return data.images[nodeId];
}

async function downloadImage(url: string) {
  return await httpApi(url).buffer();
}

async function walkNode(root: FigmaNode, callback: (node: FigmaNode) => void | Promise<unknown>, filter: (node: FigmaNode) => boolean) {
  if (filter(root)) {
    await callback(root);
    return;
  }
  if ('children' in root) {
    for (const node of root.children) {
      await walkNode(node, callback, filter);
    }
  }
}

async function walkNodeByType<T extends NodeType, TNode = FigmaNode & { type: T }>(
  root: FigmaNode,
  type: T,
  callback: (node: TNode) => void | Promise<unknown>,
) {
  await walkNode(
    root,
    async (node) => await callback(node as unknown as TNode),
    (node) => node.type === type,
  );
}

async function walkResourceComponent(document: DocumentNode, callback: (name: string, node: ComponentNode) => void | Promise<unknown>) {
  await walkNodeByType(document, 'CANVAS', async (node) => {
    if (node.name === 'Resource') {
      await walkNodeByType(node, 'FRAME', async (node) => {
        const namePrefix = node.name;
        await walkNodeByType(node, 'COMPONENT', async (node) => {
          const name = namePrefix + '/' + node.name;
          await callback(name, node);
        });
      });
    }
  });
}

function rgbToHex(color: RGB | RGBA) {
  if ('a' in color) {
    return `rgba(${(color.r * 255).toFixed(0)}, ${(color.g * 255).toFixed(0)}, ${(color.b * 255).toFixed(0)}, ${color.a.toFixed(2)})`;
  }
  return `rgba(${(color.r * 255).toFixed(0)}, ${(color.g * 255).toFixed(0)}, ${(color.b * 255).toFixed(0)})`;
}

function paintToHexColor(paint: Paint) {
  if (paint.type === 'SOLID') {
    return rgbToHex(paint.color);
  } else {
    throw new Error('not support');
  }
}

async function main() {
  const document = await (await getFile(options.file)).document;

  const theme = { dark: {}, light: {} };

  const files: { name: string; data: Buffer | string }[] = [];

  await walkResourceComponent(document, async (name, node) => {
    if (name.match(/^theme\/(light|dark)\/colors/) && node.fills instanceof Array) {
      const path = name.replace('theme/', '').split('/');
      _.set(theme, path, paintToHexColor(node.fills[0]));
    } else {
      for (const exportSetting of node.exportSettings) {
        const extname = exportSetting.format.toLowerCase();
        const imageUrl = await getExport(options.file, node.id, exportSetting);
        const buffer = await downloadImage(imageUrl);
        files.push({ name: name + '.' + extname, data: buffer });
      }
    }
  });

  const lightTheme = theme.light;
  const darkTheme = theme.dark;

  console.log('clear generated folder.');
  fs.rmSync(path.join(__dirname, './generated'), { recursive: true });

  console.log(`mkdir ${path.join(__dirname, './generated')}`);
  fs.mkdirSync(path.join(__dirname, './generated'));

  console.log(`write ${path.join(__dirname, './generated/dark.ts')}`);
  fs.writeFileSync(
    path.join(__dirname, './generated/dark.ts'),
    `/* eslint-disable */\nimport { Theme } from '../types';\n\nconst DarkTheme: Theme = ${JSON.stringify(
      darkTheme,
      null,
      2,
    )};\n\nexport default DarkTheme;\n`,
  );
  console.log(`write ${path.join(__dirname, './generated/light.ts')}`);
  fs.writeFileSync(
    path.join(__dirname, './generated/light.ts'),
    `/* eslint-disable */\nimport { Theme } from '../types';\n\nconst LightTheme: Theme = ${JSON.stringify(
      lightTheme,
      null,
      2,
    )};\n\nexport default LightTheme;\n`,
  );

  for (const file of files) {
    const fullpath = path.join(__dirname, 'generated', file.name);
    console.log(`write ${fullpath}`);
    mkdirpSync(path.dirname(fullpath));
    fs.writeFileSync(fullpath, file.data);
  }
}

if (!ACCESS_TOKEN) {
  console.log('No figma access token, skip figma generate.');
} else {
  main();
}
