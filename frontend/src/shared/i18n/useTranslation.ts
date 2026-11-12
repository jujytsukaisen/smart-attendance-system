import { useCallback } from 'react';
import { useLanguageStore } from './store';
import { translations } from './translations';
import type { TranslationDictionary } from './types';

// Dot-path accessor type helper
type PathsToStringProps<T> = T extends string
  ? []
  : {
      [K in Extract<keyof T, string>]: [K, ...PathsToStringProps<T[K]>];
    }[Extract<keyof T, string>];

type Join<T extends string[], D extends string> = T extends []
  ? never
  : T extends [infer F]
  ? F
  : T extends [infer F, ...infer R]
  ? F extends string
    ? `${F}${D}${Join<Extract<R, string[]>, D>}`
    : never
  : string;

export type TranslationKey = Join<PathsToStringProps<TranslationDictionary>, '.'>;

export function useTranslation() {
  const { language, direction, setLanguage, toggleLanguage } = useLanguageStore();

  const t = useCallback(
    (key: string, fallback?: string): string => {
      const currentDict = translations[language] || translations.ar;
      const parts = key.split('.');

      let current: any = currentDict;
      for (const part of parts) {
        if (current && typeof current === 'object' && part in current) {
          current = current[part];
        } else {
          // Fallback to English if missing in Arabic, or vice-versa
          const fallbackDict = language === 'ar' ? translations.en : translations.ar;
          let fbCurrent: any = fallbackDict;
          for (const fbPart of parts) {
            if (fbCurrent && typeof fbCurrent === 'object' && fbPart in fbCurrent) {
              fbCurrent = fbCurrent[fbPart];
            } else {
              fbCurrent = undefined;
              break;
            }
          }
          if (typeof fbCurrent === 'string') {
            return fbCurrent;
          }
          return fallback || key;
        }
      }

      if (typeof current === 'string') {
        return current;
      }

      return fallback || key;
    },
    [language]
  );

  return {
    t,
    language,
    direction,
    isRtl: direction === 'rtl',
    setLanguage,
    toggleLanguage,
    dict: translations[language] || translations.ar,
  };
}
