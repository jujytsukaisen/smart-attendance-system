import { Moon, Sun, Laptop, Check } from 'lucide-react';
import { useTheme } from 'next-themes';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/shared/components/ui/dropdown-menu';
import { useTranslation } from '@/shared/i18n';
import { cn } from '@/shared/utils';

interface ThemeToggleProps {
  className?: string;
}

export function ThemeToggle({ className }: ThemeToggleProps) {
  const { theme, setTheme, resolvedTheme } = useTheme();
  const { isRtl } = useTranslation();

  return (
    <DropdownMenu>
      <DropdownMenuTrigger
        className={cn(
          'inline-flex h-9 w-9 items-center justify-center rounded-md border border-input bg-background text-foreground shadow-2xs hover:bg-accent hover:text-accent-foreground transition-colors cursor-pointer outline-none focus-visible:ring-1 focus-visible:ring-ring',
          className
        )}
        title={isRtl ? 'المظهر (داكن / فاتح)' : 'Toggle theme (Dark / Light)'}
        aria-label="Toggle theme"
      >
        {resolvedTheme === 'dark' ? (
          <Moon className="h-4 w-4 text-indigo-400 transition-transform rotate-0 scale-100" />
        ) : (
          <Sun className="h-4 w-4 text-amber-500 transition-transform rotate-0 scale-100" />
        )}
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-40 z-50">
        <DropdownMenuItem
          onClick={() => setTheme('light')}
          className="cursor-pointer justify-between font-medium text-xs py-2"
        >
          <div className="flex items-center gap-2">
            <Sun className="h-4 w-4 text-amber-500" />
            <span>{isRtl ? 'فاتح (نهاري)' : 'Light'}</span>
          </div>
          {theme === 'light' && <Check className="h-4 w-4 text-primary" />}
        </DropdownMenuItem>

        <DropdownMenuItem
          onClick={() => setTheme('dark')}
          className="cursor-pointer justify-between font-medium text-xs py-2"
        >
          <div className="flex items-center gap-2">
            <Moon className="h-4 w-4 text-indigo-400" />
            <span>{isRtl ? 'داكن (ليلي)' : 'Dark'}</span>
          </div>
          {theme === 'dark' && <Check className="h-4 w-4 text-primary" />}
        </DropdownMenuItem>

        <DropdownMenuItem
          onClick={() => setTheme('system')}
          className="cursor-pointer justify-between font-medium text-xs py-2"
        >
          <div className="flex items-center gap-2">
            <Laptop className="h-4 w-4 text-muted-foreground" />
            <span>{isRtl ? 'تلقائي (النظام)' : 'System'}</span>
          </div>
          {theme === 'system' && <Check className="h-4 w-4 text-primary" />}
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
