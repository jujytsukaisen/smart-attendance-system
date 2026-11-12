import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { Language, Direction } from './types';

interface LanguageState {
  language: Language;
  direction: Direction;
  setLanguage: (lang: Language) => void;
  toggleLanguage: () => void;
}

const applyHtmlAttributes = (lang: Language) => {
  if (typeof document !== 'undefined') {
    const dir: Direction = lang === 'ar' ? 'rtl' : 'ltr';
    document.documentElement.lang = lang;
    document.documentElement.dir = dir;
    if (dir === 'rtl') {
      document.documentElement.classList.add('rtl');
      document.documentElement.classList.remove('ltr');
    } else {
      document.documentElement.classList.add('ltr');
      document.documentElement.classList.remove('rtl');
    }
  }
};

export const useLanguageStore = create<LanguageState>()(
  persist(
    (set, get) => ({
      language: 'ar',
      direction: 'rtl',
      setLanguage: (lang: Language) => {
        const dir: Direction = lang === 'ar' ? 'rtl' : 'ltr';
        applyHtmlAttributes(lang);
        set({ language: lang, direction: dir });
      },
      toggleLanguage: () => {
        const nextLang: Language = get().language === 'ar' ? 'en' : 'ar';
        get().setLanguage(nextLang);
      },
    }),
    {
      name: 'smart-attendance-language',
      onRehydrateStorage: () => (state) => {
        if (state?.language) {
          applyHtmlAttributes(state.language);
        } else {
          applyHtmlAttributes('ar');
        }
      },
    }
  )
);

// Apply initial language setup on page load
if (typeof document !== 'undefined') {
  try {
    const stored = localStorage.getItem('smart-attendance-language');
    if (stored) {
      const parsed = JSON.parse(stored);
      if (parsed.state?.language) {
        applyHtmlAttributes(parsed.state.language);
      } else {
        applyHtmlAttributes('ar');
      }
    } else {
      applyHtmlAttributes('ar');
    }
  } catch {
    applyHtmlAttributes('ar');
  }
}
