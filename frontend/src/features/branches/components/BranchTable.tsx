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
  const navigate = useNavigate();

  if (isLoading) {
    return (
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Branch Name</TableHead>
              <TableHead>Code</TableHead>
              <TableHead>Type</TableHead>
              <TableHead>Status</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {[...Array(5)].map((_, i) => (
              <TableRow key={i}>
                <TableCell><Skeleton className="h-4 w-[200px]" /></TableCell>
                <TableCell><Skeleton className="h-4 w-[100px]" /></TableCell>
                <TableCell><Skeleton className="h-4 w-[80px]" /></TableCell>
                <TableCell><Skeleton className="h-6 w-[80px] rounded-full" /></TableCell>
                <TableCell className="text-right"><Skeleton className="h-8 w-8 rounded-md ml-auto" /></TableCell>
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
        No branches found. Try adjusting your filters.
      </div>
    );
  }

  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Branch Name</TableHead>
            <TableHead>Code</TableHead>
            <TableHead>Type</TableHead>
            <TableHead>Status</TableHead>
            <TableHead className="text-right">Actions</TableHead>
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
                  <span className="text-xs font-semibold text-blue-600 bg-blue-100 px-2 py-1 rounded-md">Head Office</span>
                ) : (
                  <span className="text-xs text-muted-foreground">Standard</span>
                )}
              </TableCell>
              <TableCell>
                <BranchStatusBadge isActive={branch.isActive} />
              </TableCell>
              <TableCell className="text-right">
                <DropdownMenu>
                  <DropdownMenuTrigger className="h-8 w-8 p-0 flex items-center justify-center rounded-md hover:bg-accent hover:text-accent-foreground outline-none ml-auto">
                    <span className="sr-only">Open menu</span>
                    <MoreHorizontal className="h-4 w-4" />
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end">
                    <DropdownMenuLabel>Actions</DropdownMenuLabel>
                    <DropdownMenuItem onClick={() => navigate(`/branches/${branch.id}`)}>
                      <Eye className="mr-2 h-4 w-4" /> View Details
                    </DropdownMenuItem>
                    <DropdownMenuItem onClick={() => navigate(`/branches/${branch.id}/edit`)}>
                      <Edit className="mr-2 h-4 w-4" /> Edit
                    </DropdownMenuItem>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem onClick={() => onActivateToggle(branch)}>
                      {branch.isActive ? (
                        <><PowerOff className="mr-2 h-4 w-4" /> Deactivate</>
                      ) : (
                        <><Power className="mr-2 h-4 w-4" /> Activate</>
                      )}
                    </DropdownMenuItem>
                    <DropdownMenuItem onClick={() => onRestoreClick(branch.id)}>
                      <RotateCcw className="mr-2 h-4 w-4" /> Restore
                    </DropdownMenuItem>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem 
                      onClick={() => onDeleteClick(branch.id)}
                      className="text-destructive focus:text-destructive"
                    >
                      <Trash2 className="mr-2 h-4 w-4" /> Delete
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
