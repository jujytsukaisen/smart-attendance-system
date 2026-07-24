import { Activity, Wifi, ShieldCheck, Database, RefreshCw, Upload } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/shared/components/ui/card';
import { Button } from '@/shared/components/ui/button';
import { ConnectionStatusBadge } from './DeviceStatusBadge';
import type { AttendanceDeviceDetailsDto } from '../types';
import { CommunicationProtocol } from '../types';

interface DeviceConnectionCardProps {
  device: AttendanceDeviceDetailsDto;
  onTestConnection: () => void;
  onRestart: () => void;
  onUploadUsers: () => void;
  onUploadPolicies: () => void;
  isTesting?: boolean;
  isRestarting?: boolean;
}

export function DeviceConnectionCard({ 
  device, 
  onTestConnection, 
  onRestart,
  onUploadUsers,
  onUploadPolicies,
  isTesting,
  isRestarting
}: DeviceConnectionCardProps) {
  const protocolName = Object.keys(CommunicationProtocol).find(
    (key) => CommunicationProtocol[key as keyof typeof CommunicationProtocol] === device.communicationProtocol
  ) || 'Unknown';

  return (
    <Card>
      <CardHeader className="pb-4">
        <div className="flex items-center justify-between">
          <div>
            <CardTitle className="text-lg flex items-center gap-2">
              <Wifi className="h-5 w-5 text-gray-500" />
              Network & Connection
            </CardTitle>
            <CardDescription className="mt-1">
              Live status and networking configuration.
            </CardDescription>
          </div>
          <ConnectionStatusBadge status={device.connectionStatus} />
        </div>
      </CardHeader>
      <CardContent className="space-y-6">
        <div className="grid grid-cols-2 gap-4 text-sm">
          <div>
            <span className="block text-gray-500">IP Address</span>
            <span className="font-medium text-gray-900">{device.ipAddress || 'Not Configured'}</span>
          </div>
          <div>
            <span className="block text-gray-500">Port</span>
            <span className="font-medium text-gray-900">{device.port || 'Not Configured'}</span>
          </div>
          <div>
            <span className="block text-gray-500">Protocol</span>
            <span className="font-medium text-gray-900">{protocolName}</span>
          </div>
          <div>
            <span className="block text-gray-500">Secure Comm</span>
            <span className="font-medium text-gray-900 flex items-center gap-1">
              {device.requireSecureConnection ? (
                <><ShieldCheck className="h-4 w-4 text-green-500" /> Enabled</>
              ) : (
                'Disabled'
              )}
            </span>
          </div>
        </div>

        <div className="border-t border-gray-100 pt-4 flex flex-wrap gap-2">
          <Button 
            variant="outline" 
            size="sm" 
            onClick={onTestConnection} 
            disabled={isTesting}
          >
            <Activity className="h-4 w-4 mr-2" />
            {isTesting ? 'Testing...' : 'Test Connection'}
          </Button>
          
          <Button 
            variant="outline" 
            size="sm" 
            onClick={onRestart} 
            disabled={isRestarting}
          >
            <RefreshCw className="h-4 w-4 mr-2" />
            {isRestarting ? 'Restarting...' : 'Restart Device'}
          </Button>

          <Button 
            variant="outline" 
            size="sm" 
            onClick={onUploadUsers} 
          >
            <Upload className="h-4 w-4 mr-2" />
            Push Users
          </Button>

          <Button 
            variant="outline" 
            size="sm" 
            onClick={onUploadPolicies} 
          >
            <Database className="h-4 w-4 mr-2" />
            Push Policies
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
