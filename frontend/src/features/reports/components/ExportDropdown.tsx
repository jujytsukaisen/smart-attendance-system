import { Download, FileText, FileSpreadsheet, FileIcon } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/shared/components/ui/dropdown-menu';
import { useExportReport } from '../hooks';
import type { ReportFilterDto } from '../types';
import { toast } from 'sonner';

interface ExportDropdownProps {
  filter: ReportFilterDto;
}

export function ExportDropdown({ filter }: ExportDropdownProps) {
  const exportMutation = useExportReport();

  const handleExport = (type: 'pdf' | 'csv' | 'excel') => {
    toast.promise(exportMutation.mutateAsync({ type, filter }), {
      loading: `Generating ${type.toUpperCase()}...`,
      success: `${type.toUpperCase()} exported successfully!`,
      error: `Failed to export ${type.toUpperCase()}.`,
    });
  };

  return (
    <DropdownMenu>
      <DropdownMenuTrigger>
        <Button variant="outline" disabled={exportMutation.isPending} className="pointer-events-none">
          <Download className="h-4 w-4 mr-2" />
          Export
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuItem onClick={() => handleExport('pdf')}>
          <FileText className="h-4 w-4 mr-2 text-red-500" />
          Export as PDF
        </DropdownMenuItem>
        <DropdownMenuItem onClick={() => handleExport('excel')}>
          <FileSpreadsheet className="h-4 w-4 mr-2 text-green-600" />
          Export as Excel
        </DropdownMenuItem>
        <DropdownMenuItem onClick={() => handleExport('csv')}>
          <FileIcon className="h-4 w-4 mr-2 text-blue-500" />
          Export as CSV
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
