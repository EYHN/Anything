import { i18n } from '@lingui/core';
import { useLingui } from '@lingui/react';

import { LocaleMessages } from '@anything/shared';
import { messages as messagesEn } from '@anything/shared/locale/en/messages.po';
import { messages as messagesZhCN } from '@anything/shared/locale/zh-CN/messages.po';
import { en, zh } from 'make-plural/plurals';
import { useCallback } from 'react';

i18n.loadLocaleData('en', { plurals: en });
i18n.loadLocaleData('zh', { plurals: zh });
i18n.loadLocaleData('zh-CN', { plurals: zh });

i18n.load('en', messagesEn);
i18n.load('zh-CN', messagesZhCN);

i18n.activate('en');

export function useI18n() {
  const { i18n } = useLingui();

  const localeMimetype = useCallback(
    (mime = 'other', message = 'File') => {
      return i18n._('MimeType:' + mime, undefined, { message });
    },
    [i18n],
  );

  const localeMetadata = useCallback(
    (metadataKey: string) => {
      return i18n._('Metadata.' + metadataKey, undefined, { message: metadataKey });
    },
    [i18n],
  );

  const localeMetadataValue = useCallback(
    (metadataKey: string, value: string | number) => {
      return i18n._('Metadata.' + metadataKey + '.Value', { value }, { message: value.toString() });
    },
    [i18n],
  );

  const localeUI = useCallback(
    (ui: typeof LocaleMessages[number] & `UI.${string}`) => {
      return i18n._(ui);
    },
    [i18n],
  );

  return {
    i18n,
    localeMimetype,
    localeMetadata,
    localeMetadataValue,
    localeUI,
  };
}

export default i18n;
