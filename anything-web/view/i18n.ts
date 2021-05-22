import { i18n } from "@lingui/core";
import { useLingui } from "@lingui/react";

import messagesEn from 'locale/en/messages';
import messagesZhCN from 'locale/zh-CN/messages';
import { en, zh } from "make-plural/plurals"
import { useCallback } from "react";

i18n.loadLocaleData("en", { plurals: en })
i18n.loadLocaleData("zh", { plurals: zh })
i18n.loadLocaleData("zh-CN", { plurals: zh })

i18n.load('en', messagesEn);
i18n.load('zh-CN', messagesZhCN);

i18n.activate('zh-CN')

export function useI18n() {
  const { i18n } = useLingui();

  const localeMimetype = useCallback((mime: string = 'other', message: string = 'File') => {
    return i18n._('MimeType:' + mime, undefined, { message })
  }, [i18n]);

  const localeMetadata = useCallback((metadataKey: string, message?: string) => {
    return i18n._("Metadata." + metadataKey, undefined, { message: message || metadataKey })
  }, [i18n]);

  const localeMetadataValue = useCallback((metadataKey: string, value: string | number, message?: string) => {
    return i18n._("Metadata." + metadataKey + ".Value", { value }, { message: message })
  }, [i18n]);

  return {
    i18n,
    localeMimetype,
    localeMetadata,
    localeMetadataValue
  }
}

export default i18n;