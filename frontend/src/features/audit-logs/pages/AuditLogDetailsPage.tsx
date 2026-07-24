import { useParams, useNavigate } from 'react-router-dom';
import { format } from 'date-fns';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/shared/components/ui/card';
import { Button } from '@/shared/components/ui/button';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { Badge } from '@/shared/components/ui/badge';
import { useAuditLog } from '../hooks';
import { ArrowLeft, User, MapPin, Globe, Activity, Database, AlertCircle } from 'lucide-react';

export function AuditLogDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data, isLoading, error, isError } = useAuditLog(id!);

  if (isLoading) {
    return <div className="max-w-4xl mx-auto p-4"><Skeleton className="h-[500px]" /></div>;
  }

  if (error || !data) {
  if (isError) {
    return (
      <div className="flex items-center justify-center py-10">
        <p className="text-destructive">
          {error instanceof Error && !error.message.toLowerCase().includes('internal server error') && !error.message.includes('status code')
            ? error.message
            : "Failed to load"}
        </p>
      </div>
    );
  }


    return (
      <div className="max-w-4xl mx-auto p-4 text-center">
        <h2 className="text-xl font-bold text-gray-900">Audit log not found</h2>
        <Button variant="link" onClick={() => navigate('/audit-logs')}>Return to audit logs</Button>
      </div>
    );
  }

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      <Button variant="ghost" onClick={() => navigate('/audit-logs')} className="mb-2">
        <ArrowLeft className="h-4 w-4 mr-2" /> Back to audit logs
      </Button>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card className="md:col-span-2">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Activity className="h-5 w-5 text-gray-500" />
              Action Details
            </CardTitle>
            <CardDescription>
              {format(new Date(data.createdAt), 'MMMM do, yyyy HH:mm:ss.SSS')}
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
             <div className="flex flex-wrap gap-4 mb-4">
                <div>
                   <div className="text-xs text-gray-500 mb-1">Action</div>
                   <Badge variant="outline" className="text-sm font-medium">{data.action}</Badge>
                </div>
                <div>
                   <div className="text-xs text-gray-500 mb-1">Entity</div>
                   <Badge variant="outline" className="text-sm font-medium">{data.entityName}</Badge>
                </div>
                <div>
                   <div className="text-xs text-gray-500 mb-1">Status</div>
                   <Badge variant={data.result === 'Success' ? 'default' : 'destructive'} className="text-sm font-medium">{data.result}</Badge>
                </div>
             </div>

             {data.errorMessage && (
               <div className="p-4 bg-red-50 text-red-800 rounded-md flex gap-2">
                 <AlertCircle className="h-5 w-5 text-red-500 flex-shrink-0" />
                 <span className="text-sm">{data.errorMessage}</span>
               </div>
             )}

             <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
               {data.oldValues && (
                 <div className="space-y-2">
                   <div className="text-sm font-semibold text-gray-900">Before</div>
                   <pre className="bg-gray-50 p-4 rounded-md text-xs text-gray-800 overflow-x-auto border">
                     {JSON.stringify(JSON.parse(data.oldValues), null, 2)}
                   </pre>
                 </div>
               )}
               {data.newValues && (
                 <div className="space-y-2">
                   <div className="text-sm font-semibold text-gray-900">After</div>
                   <pre className="bg-green-50 p-4 rounded-md text-xs text-green-900 overflow-x-auto border border-green-100">
                     {JSON.stringify(JSON.parse(data.newValues), null, 2)}
                   </pre>
                 </div>
               )}
             </div>
          </CardContent>
        </Card>

        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="text-base flex items-center gap-2">
                <User className="h-4 w-4 text-gray-500" /> User Context
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-sm">
              <div>
                <div className="text-gray-500 text-xs">User Name</div>
                <div className="font-medium text-gray-900">{data.userName}</div>
              </div>
              <div>
                <div className="text-gray-500 text-xs">User ID</div>
                <div className="font-mono text-gray-900 text-xs mt-1">{data.userId}</div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="text-base flex items-center gap-2">
                <Globe className="h-4 w-4 text-gray-500" /> Network Context
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-sm">
              <div className="flex gap-2 items-start">
                <MapPin className="h-4 w-4 text-gray-400 mt-0.5" />
                <div>
                  <div className="text-gray-500 text-xs">IP Address</div>
                  <div className="font-mono text-gray-900">{data.ipAddress}</div>
                </div>
              </div>
              <div className="flex gap-2 items-start">
                <Database className="h-4 w-4 text-gray-400 mt-0.5" />
                <div>
                  <div className="text-gray-500 text-xs">Browser / User Agent</div>
                  <div className="text-gray-900 break-words">{data.browser}</div>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
