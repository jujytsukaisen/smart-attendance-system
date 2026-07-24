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
import { CompanyStatusBadge } from './CompanyStatusBadge';
import { useNavigate } from 'react-router-dom';
import type { CompanyListDto } from '../types';

interface CompanyTableProps {
  data: CompanyListDto[];
  isLoading: boolean;
  onDeleteClick: (id: string) => void;
  onRestoreClick: (id: string) => void;
  onActivateToggle: (company: CompanyListDto) => void;
}

export const CompanyTable = ({ 
  data, 
  isLoading, 
  onDeleteClick, 
  onRestoreClick,
  onActivateToggle
}: CompanyTableProps) => {
  const navigate = useNavigate();

  if (isLoading) {
    return (
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Company</TableHead>
              <TableHead>Registration Number</TableHead>
              <TableHead>Contact</TableHead>
              <TableHead>Status</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {[...Array(5)].map((_, i) => (
              <TableRow key={i}>
                <TableCell><Skeleton className="h-4 w-[200px]" /></TableCell>
                <TableCell><Skeleton className="h-4 w-[100px]" /></TableCell>
                <TableCell><Skeleton className="h-4 w-[150px]" /></TableCell>
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
        No companies found. Try adjusting your filters.
      </div>
    );
  }

  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Company</TableHead>
            <TableHead>Registration Number</TableHead>
            <TableHead>Contact</TableHead>
            <TableHead>Status</TableHead>
            <TableHead className="text-right">Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {data.map((company) => (
            <TableRow key={company.id}>
              <TableCell className="font-medium">
                <div className="flex items-center gap-2">
                  {company.logoUrl && (
                    <img src={company.logoUrl} alt={company.name} className="w-6 h-6 rounded-md object-cover" />
                  )}
                  {company.name}
                </div>
              </TableCell>
              <TableCell>{company.registrationNumber}</TableCell>
              <TableCell>
                <div className="flex flex-col text-xs text-muted-foreground">
                  <span>{company.email}</span>
                  <span>{company.phoneNumber}</span>
                </div>
              </TableCell>
              <TableCell>
                <CompanyStatusBadge isActive={company.isActive} />
              </TableCell>
              <TableCell className="text-right">
                <DropdownMenu>
                  <DropdownMenuTrigger className="h-8 w-8 p-0 flex items-center justify-center rounded-md hover:bg-accent hover:text-accent-foreground outline-none">
                    <span className="sr-only">Open menu</span>
                    <MoreHorizontal className="h-4 w-4" />
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end">
                    <DropdownMenuLabel>Actions</DropdownMenuLabel>
                    <DropdownMenuItem onClick={() => navigate(`/companies/${company.id}`)}>
                      <Eye className="mr-2 h-4 w-4" /> View Details
                    </DropdownMenuItem>
                    <DropdownMenuItem onClick={() => navigate(`/companies/${company.id}/edit`)}>
                      <Edit className="mr-2 h-4 w-4" /> Edit
                    </DropdownMenuItem>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem onClick={() => onActivateToggle(company)}>
                      {company.isActive ? (
                        <><PowerOff className="mr-2 h-4 w-4" /> Deactivate</>
                      ) : (
                        <><Power className="mr-2 h-4 w-4" /> Activate</>
                      )}
                    </DropdownMenuItem>
                    <DropdownMenuItem onClick={() => onRestoreClick(company.id)}>
                      <RotateCcw className="mr-2 h-4 w-4" /> Restore
                    </DropdownMenuItem>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem 
                      onClick={() => onDeleteClick(company.id)}
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
