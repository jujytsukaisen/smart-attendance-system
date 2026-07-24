import { Button } from '@/shared/components/ui/button';
import { Loader2 } from 'lucide-react';

interface SettingsSaveBarProps {
  isDirty: boolean;
  isSaving: boolean;
  onReset: () => void;
  onSave: () => void;
}

export function SettingsSaveBar({ isDirty, isSaving, onReset, onSave }: SettingsSaveBarProps) {
  if (!isDirty) return null;

  return (
    <div className="fixed bottom-0 left-0 right-0 z-50 p-4 bg-white border-t shadow-lg flex justify-center animate-in slide-in-from-bottom-2">
      <div className="flex items-center justify-between w-full max-w-4xl bg-gray-900 text-white px-6 py-3 rounded-lg shadow-xl">
        <span className="text-sm font-medium">You have unsaved changes.</span>
        <div className="flex gap-3">
          <Button variant="ghost" size="sm" onClick={onReset} disabled={isSaving} className="text-gray-300 hover:text-white hover:bg-gray-800">
            Discard
          </Button>
          <Button size="sm" onClick={onSave} disabled={isSaving} className="bg-white text-gray-900 hover:bg-gray-100">
            {isSaving ? <Loader2 className="h-4 w-4 mr-2 animate-spin" /> : null}
            Save Changes
          </Button>
        </div>
      </div>
    </div>
  );
}
