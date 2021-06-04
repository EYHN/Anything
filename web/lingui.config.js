module.exports = {
  locales: ['en', 'zh-CN'],
  sourceLocale: 'en',
  format: 'po',
  compileNamespace: 'es',
  rootDir: '../locale',
  catalogs: [
    {
      path: '<rootDir>/{locale}/messages',
      exclude: ['**/node_modules/**'],
    },
  ],
};
