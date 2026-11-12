import { Check } from 'lucide-react';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/shared/components/ui/dropdown-menu';
import { useLanguageStore } from '@/shared/i18n';
import { cn } from '@/shared/utils';

const ArabicEnglishIcon = ({ className }: { className?: string }) => (
  <svg 
    xmlns="http://www.w3.org/2000/svg" 
    width="24" height="24" viewBox="0 0 24 24" 
    fill="none" stroke="currentColor" strokeWidth="2" 
    strokeLinecap="round" strokeLinejoin="round" 
    className={className}
  >
    <path d="m22 22-5-10-5 10"/>
    <path d="M14 18h6"/>
    <text x="4" y="16" stroke="none" fill="currentColor" fontSize="16" fontFamily="Arial, Helvetica, sans-serif" fontWeight="bold">ع</text>
  </svg>
);

interface LanguageSwitcherProps {
  variant?: 'default' | 'outline' | 'ghost' | 'secondary';
  size?: 'default' | 'sm' | 'lg' | 'icon';
  showLabel?: boolean;
  className?: string;
}

export function LanguageSwitcher({
  showLabel = true,
  className,
}: LanguageSwitcherProps) {
  const { language, setLanguage } = useLanguageStore();

  return (
    <DropdownMenu>
      <DropdownMenuTrigger
        className={cn(
          'inline-flex items-center justify-center gap-2 rounded-md border border-input bg-background px-3 py-1.5 text-xs font-semibold shadow-2xs hover:bg-accent hover:text-accent-foreground transition-all cursor-pointer outline-none focus-visible:ring-1 focus-visible:ring-ring',
          className
        )}
        title="تغيير اللغة / Change Language"
      >
        <ArabicEnglishIcon className="h-4 w-4 text-primary" />
        {showLabel && (
          <span>
            {language === 'ar' ? 'العربية' : 'English'}
          </span>
        )}
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-36 z-50">
        <DropdownMenuItem
          onClick={() => setLanguage('ar')}
          className="cursor-pointer justify-between font-medium text-xs py-2"
        >
          <div className="flex items-center gap-2">
            <span>🇸🇦</span>
            <span>العربية</span>
          </div>
          {language === 'ar' && <Check className="h-4 w-4 text-primary" />}
        </DropdownMenuItem>

        <DropdownMenuItem
          onClick={() => setLanguage('en')}
          className="cursor-pointer justify-between font-medium text-xs py-2"
        >
          <div className="flex items-center gap-2">
            <span>🇺🇸</span>
            <span>English</span>
          </div>
          {language === 'en' && <Check className="h-4 w-4 text-primary" />}
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
