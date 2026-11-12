import { 
  Table, 
  TableBody, 
  TableCell, 
  TableHead, 
  TableHeader, 
  TableRow 
} from '@/shared/components/ui/table';
import { 
  DropdownMenu, 
  DropdownMenuContent, 
  DropdownMenuItem, 
  DropdownMenuLabel, 
  DropdownMenuSeparator, 
  DropdownMenuTrigger 
} from '@/shared/components/ui/dropdown-menu';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { MoreHorizontal, Eye, Edit, Trash2, RotateCcw, Power, PowerOff } from 'lucide-react';
import { BranchStatusBadge } from './BranchStatusBadge';
import { useNavigate } from 'react-router-dom';
import type { BranchListDto } from '../types';
import { useTranslation } from '@/shared/i18n';

interface BranchTableProps {
  data: BranchListDto[];
  isLoading: boolean;
  onDeleteClick: (id: string) => void;
  onRestoreClick: (id: string) => void;
  onActivateToggle: (branch: BranchListDto) => void;
}

export const BranchTable = ({ 
  data, 
  isLoading, 
  onDeleteClick, 
  onRestoreClick,
  onActivateToggle
}: BranchTableProps) => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  if (isLoading) {
    return (
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{t('branches.branchName')}</TableHead>
              <TableHead>{t('branches.code')}</TableHead>
              <TableHead>{t('branches.type')}</TableHead>
              <TableHead>{t('branches.status')}</TableHead>
              <TableHead className="w-[80px] text-center">{t('branches.actions')}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {[...Array(5)].map((_, i) => (
              <TableRow key={i}>
                <TableCell><Skeleton className="h-4 w-[200px]" /></TableCell>
                <TableCell><Skeleton className="h-4 w-[100px]" /></TableCell>
                <TableCell><Skeleton className="h-4 w-[80px]" /></TableCell>
                <TableCell><Skeleton className="h-6 w-[80px] rounded-full" /></TableCell>
                <TableCell className="text-center"><Skeleton className="h-8 w-8 rounded-md mx-auto" /></TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    );
  }

  if (!data.length) {
    return (
      <div className="rounded-md border p-8 text-center text-muted-foreground">
        {t('branches.noBranchesFound')}
      </div>
    );
  }

  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>{t('branches.branchName')}</TableHead>
            <TableHead>{t('branches.code')}</TableHead>
            <TableHead>{t('branches.type')}</TableHead>
            <TableHead>{t('branches.status')}</TableHead>
            <TableHead className="w-[80px] text-center">{t('branches.actions')}</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {data.map((branch) => (
            <TableRow key={branch.id}>
              <TableCell className="font-medium">
                {branch.name}
              </TableCell>
              <TableCell>{branch.code}</TableCell>
              <TableCell>
                {branch.isHeadOffice ? (
                  <span className="text-xs font-semibold text-blue-600 bg-blue-100 dark:bg-blue-950 dark:text-blue-400 px-2 py-1 rounded-md">
                    {t('branches.headOffice')}
                  </span>
                ) : (
                  <span className="text-xs text-muted-foreground">
                    {t('branches.standard')}
                  </span>
                )}
              </TableCell>
              <TableCell>
                <BranchStatusBadge isActive={branch.isActive} />
              </TableCell>
              <TableCell className="text-center">
                <DropdownMenu>
                  <DropdownMenuTrigger className="h-8 w-8 p-0 inline-flex items-center justify-center rounded-md hover:bg-accent hover:text-accent-foreground outline-none">
                    <span className="sr-only">{t('common.actions')}</span>
                    <MoreHorizontal className="h-4 w-4" />
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end">
                    <DropdownMenuLabel>{t('branches.actions')}</DropdownMenuLabel>
                    <DropdownMenuItem onClick={() => navigate(`/branches/${branch.id}`)}>
                      <Eye className="h-4 w-4 ltr:mr-2 rtl:ml-2" /> {t('common.details')}
                    </DropdownMenuItem>
                    <DropdownMenuItem onClick={() => navigate(`/branches/${branch.id}/edit`)}>
                      <Edit className="h-4 w-4 ltr:mr-2 rtl:ml-2" /> {t('common.edit')}
                    </DropdownMenuItem>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem onClick={() => onActivateToggle(branch)}>
                      {branch.isActive ? (
                        <><PowerOff className="h-4 w-4 ltr:mr-2 rtl:ml-2" /> {t('employees.deactivate')}</>
                      ) : (
                        <><Power className="h-4 w-4 ltr:mr-2 rtl:ml-2" /> {t('employees.activate')}</>
                      )}
                    </DropdownMenuItem>
                    <DropdownMenuItem onClick={() => onRestoreClick(branch.id)}>
                      <RotateCcw className="h-4 w-4 ltr:mr-2 rtl:ml-2" /> {t('employees.restore')}
                    </DropdownMenuItem>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem 
                      onClick={() => onDeleteClick(branch.id)}
                      className="text-destructive focus:text-destructive"
                    >
                      <Trash2 className="h-4 w-4 ltr:mr-2 rtl:ml-2" /> {t('common.delete')}
                    </DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
};
