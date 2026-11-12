import { Download, FileText, FileSpreadsheet, FileIcon } from 'lucide-react';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/shared/components/ui/dropdown-menu';
import { useExportReport } from '../hooks';
import type { ReportFilterDto } from '../types';
import { toast } from 'sonner';
import { useTranslation } from '@/shared/i18n';

interface ExportDropdownProps {
  filter: ReportFilterDto;
}

export function ExportDropdown({ filter }: ExportDropdownProps) {
  const { t } = useTranslation();
  const exportMutation = useExportReport();

  const handleExport = (type: 'pdf' | 'csv' | 'excel') => {
    toast.promise(exportMutation.mutateAsync({ type, filter }), {
      loading: `${t('common.loading')}`,
      success: `${t('common.success')}`,
      error: `${t('common.error')}`,
    });
  };

  return (
    <DropdownMenu>
      <DropdownMenuTrigger
        disabled={exportMutation.isPending}
        className="inline-flex items-center justify-center gap-2 rounded-md border border-input bg-background px-3 py-2 text-sm font-medium shadow-xs hover:bg-accent hover:text-accent-foreground transition-colors cursor-pointer outline-none disabled:pointer-events-none disabled:opacity-50"
      >
        <Download className="h-4 w-4" />
        {t('reports.export')}
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuItem onClick={() => handleExport('pdf')}>
          <FileText className="h-4 w-4 ltr:mr-2 rtl:ml-2 text-red-500" />
          {t('reports.exportPdf')}
        </DropdownMenuItem>
        <DropdownMenuItem onClick={() => handleExport('excel')}>
          <FileSpreadsheet className="h-4 w-4 ltr:mr-2 rtl:ml-2 text-emerald-600" />
          {t('reports.exportExcel')}
        </DropdownMenuItem>
        <DropdownMenuItem onClick={() => handleExport('csv')}>
          <FileIcon className="h-4 w-4 ltr:mr-2 rtl:ml-2 text-blue-500" />
          {t('reports.exportCsv')}
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
