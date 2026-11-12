import { Button } from '@/shared/components/ui/button';
import { Loader2 } from 'lucide-react';
import { useTranslation } from '@/shared/i18n';

interface SettingsSaveBarProps {
  isDirty: boolean;
  isSaving: boolean;
  onReset: () => void;
  onSave: () => void;
}

export function SettingsSaveBar({ isDirty, isSaving, onReset, onSave }: SettingsSaveBarProps) {
  const { t } = useTranslation();
  if (!isDirty) return null;

  return (
    <div className="fixed bottom-0 left-0 right-0 z-50 p-4 bg-background/80 backdrop-blur-md border-t shadow-lg flex justify-center animate-in slide-in-from-bottom-2">
      <div className="flex items-center justify-between w-full max-w-4xl bg-card border text-card-foreground px-6 py-3 rounded-lg shadow-xl">
        <span className="text-sm font-medium">
          {t('settings.subtitle')}
        </span>
        <div className="flex gap-3">
          <Button variant="ghost" size="sm" onClick={onReset} disabled={isSaving}>
            {t('settings.reset')}
          </Button>
          <Button size="sm" onClick={onSave} disabled={isSaving}>
            {isSaving ? <Loader2 className="h-4 w-4 ltr:mr-2 rtl:ml-2 animate-spin" /> : null}
            {t('settings.saveChanges')}
          </Button>
        </div>
      </div>
    </div>
  );
}
