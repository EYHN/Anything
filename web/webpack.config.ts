import path from 'path';
import webpack from 'webpack';
import HtmlWebpackPlugin from 'html-webpack-plugin';

// eslint-disable-next-line @typescript-eslint/no-var-requires
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;

const svgoConfig = {
  plugins: [
    { cleanupAttrs: true },
    { cleanupEnableBackground: true },
    { cleanupIDs: true },
    { cleanupListOfValues: true },
    { cleanupNumericValues: true },
    { collapseGroups: true },
    { convertColors: false },
    { convertPathData: true },
    { convertShapeToPath: true },
    { convertStyleToAttrs: true },
    { convertTransform: true },
    { mergePaths: true },
    { moveElemsAttrsToGroup: true },
    { moveGroupAttrsToElems: true },
    { removeComments: true },
    { removeDesc: true },
    { removeDimensions: true },
    { removeDoctype: true },
    { removeEditorsNSData: true },
    { removeEmptyAttrs: true },
    { removeEmptyContainers: true },
    { removeEmptyText: true },
    { removeHiddenElems: true },
    { removeMetadata: true },
    { removeNonInheritableGroupAttrs: true },
    { removeRasterImages: true },
    { removeTitle: true },
    { removeUnknownsAndDefaults: true },
    { removeUselessDefs: true },
    { removeUnusedNS: true },
    { removeUselessStrokeAndFill: true },
    { removeXMLProcInst: true },
    { sortAttrs: true },
    { removeAttrs: { attrs: ['svg:height', 'svg:width'] } },
  ],
};

const HtmlWebpackConfig: HtmlWebpackPlugin.Options = {
  title: 'anything',
  filename: 'index.html',
  hash: true,
  showErrors: true,
};

const babelOptions = {
  babelrc: false,
  presets: [
    [
      '@babel/preset-env',
      {
        targets: {
          browsers: ['last 2 versions', 'safari >= 7'],
        },
      },
    ],
    '@babel/preset-react',
  ],
  plugins: ['@babel/plugin-transform-runtime'],
};

const css = {
  test: /\.css$/,
  use: [
    'to-string-loader',
    {
      loader: 'css-loader',
      options: {
        esModule: false,
      },
    },
  ],
};

const image = {
  test: /\.(png|jpg|jpeg|gif|bmp)$/,
  use: [
    {
      loader: 'file-loader',
      options: {
        name: '[path][name].[ext]',
        outputPath: 'images/',
        esModule: false,
      },
    },
  ],
};

const typescript = {
  test: /\.(ts|tsx)$/,
  use: [
    {
      loader: 'babel-loader',
      options: babelOptions,
    },
    {
      loader: 'ts-loader',
      options: {
        configFile: path.resolve(__dirname, './tsconfig.json'),
      },
    },
  ],
};

const po = {
  test: /\.po$/,
  use: [
    {
      loader: '@lingui/loader',
      options: {
        config: path.resolve(__dirname, './lingui.config.js'),
      },
    },
  ],
};

const svg = {
  test: /\.svg$/,
  use: [
    {
      loader: '@svgr/webpack',
      options: {
        icon: true,
        svgoConfig,
      },
    },
  ],
};

const performanceAssetFilter = (assetFilename: string): boolean => {
  return assetFilename.endsWith('.js');
};

const config: webpack.Configuration = {
  name: 'test',
  mode: process.env.NODE_ENV === 'production' ? 'production' : 'development',
  context: __dirname,
  target: 'web',
  entry: [path.resolve(__dirname, './main.tsx')],
  output: {
    filename: '[name].js',
    publicPath: '/',
    path: path.resolve(__dirname, './dist/'),
  },

  devtool: 'source-map',

  plugins: [
    new HtmlWebpackPlugin(HtmlWebpackConfig),
    new BundleAnalyzerPlugin({
      openAnalyzer: false,
      analyzerMode: 'static',
      reportFilename: './stats.html',
    }),
  ],

  resolve: {
    extensions: ['.ts', '.tsx', '.js', '.jsx'],
    modules: [__dirname, 'node_modules'],
  },
  module: {
    rules: [typescript, css, image, po, svg],
  },
  devServer: {
    port: parseInt(process.env.PORT || '8888'),
    host: '127.0.0.1',
    publicPath: '/',
    contentBase: __dirname,
    historyApiFallback: true,
    open: true,
    hot: true,
    inline: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
      },
    },
    headers: {
      'access-control-allow-origin': '*',
    },
  },
  performance: {
    assetFilter: performanceAssetFilter,
    maxAssetSize: 300000,
    maxEntrypointSize: 300000,
  },
  optimization: {
    runtimeChunk: true,
  },
};

export default config;
