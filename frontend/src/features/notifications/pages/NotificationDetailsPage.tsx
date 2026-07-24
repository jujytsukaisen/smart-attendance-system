import { useParams, useNavigate, Link } from 'react-router-dom';
import { useEffect } from 'react';
import { format } from 'date-fns';
import { Card, CardContent, CardHeader, CardTitle, CardFooter } from '@/shared/components/ui/card';
import { Button } from '@/shared/components/ui/button';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { useNotification, useMarkAsRead, useDeleteNotification } from '../hooks';
import { ArrowLeft, Trash2 } from 'lucide-react';
import { toast } from 'sonner';

export function NotificationDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data, isLoading, error, isError } = useNotification(id!);
  const markAsRead = useMarkAsRead();
  const deleteNotification = useDeleteNotification();

  useEffect(() => {
    if (data && !data.isRead) {
      markAsRead.mutate(data.id);
    }
  }, [data, markAsRead]);

  const handleDelete = async () => {
    if (!id) return;
    await deleteNotification.mutateAsync(id);
    toast.success('Notification deleted');
    navigate('/notifications');
  };

  if (isLoading) {
    return <div className="max-w-2xl mx-auto p-4"><Skeleton className="h-64" /></div>;
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
      <div className="max-w-2xl mx-auto p-4 text-center">
        <h2 className="text-xl font-bold text-gray-900">Notification not found</h2>
        <Button variant="link" onClick={() => navigate('/notifications')}>Return to notifications</Button>
      </div>
    );
  }

  return (
    <div className="space-y-6 max-w-2xl mx-auto">
      <Button variant="ghost" onClick={() => navigate('/notifications')} className="mb-2">
        <ArrowLeft className="h-4 w-4 mr-2" /> Back to notifications
      </Button>

      <Card>
        <CardHeader className="border-b bg-gray-50/50">
          <div className="flex justify-between items-start">
            <div>
              <CardTitle className="text-xl text-gray-900">{data.title}</CardTitle>
              <div className="text-xs text-gray-500 mt-2">
                {format(new Date(data.createdAt), 'EEEE, MMMM do, yyyy h:mm a')}
              </div>
            </div>
          </div>
        </CardHeader>
        <CardContent className="pt-6 pb-8 min-h-[150px]">
          <p className="text-gray-800 text-sm md:text-base leading-relaxed">{data.message}</p>
        </CardContent>
        <CardFooter className="bg-gray-50 border-t flex justify-between py-3">
          <Button variant="outline" className="text-red-600 hover:text-red-700 hover:bg-red-50" onClick={handleDelete}>
            <Trash2 className="h-4 w-4 mr-2" /> Delete
          </Button>
          {data.actionUrl && (
            <Link to={data.actionUrl}>
              <Button>View Details</Button>
            </Link>
          )}
        </CardFooter>
      </Card>
    </div>
  );
}
