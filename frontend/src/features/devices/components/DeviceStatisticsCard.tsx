import { formatDistanceToNow } from 'date-fns';
import { Clock, Cpu, MapPin, Search } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/shared/components/ui/card';
import { Badge } from '@/shared/components/ui/badge';
import type { AttendanceDeviceDetailsDto } from '../types';

export function DeviceStatisticsCard({ device }: { device: AttendanceDeviceDetailsDto }) {
  const capabilities = [
    { name: 'Face Recognition', enabled: device.supportsFaceRecognition },
    { name: 'Fingerprint', enabled: device.supportsFingerprint },
    { name: 'RFID/NFC', enabled: device.supportsNFC },
    { name: 'QR Code', enabled: device.supportsQRCode },
    { name: 'GPS Location', enabled: device.supportsGPS },
    { name: 'Offline Mode', enabled: device.supportsOfflineAttendance },
  ];

  return (
    <Card>
      <CardHeader className="pb-4">
        <CardTitle className="text-lg flex items-center gap-2">
          <Cpu className="h-5 w-5 text-gray-500" />
          Hardware & Capabilities
        </CardTitle>
        <CardDescription className="mt-1">
          Device features and synchronization stats.
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        <div className="space-y-3 border-b border-gray-100 pb-4">
          <div className="flex items-center gap-3">
            <Clock className="h-4 w-4 text-gray-400" />
            <div className="text-sm">
              <span className="text-gray-500 mr-2">Last Heartbeat:</span>
              <span className="font-medium text-gray-900">
                {device.lastHeartbeat ? formatDistanceToNow(new Date(device.lastHeartbeat), { addSuffix: true }) : 'Never'}
              </span>
            </div>
          </div>
          <div className="flex items-center gap-3">
            <Search className="h-4 w-4 text-gray-400" />
            <div className="text-sm">
              <span className="text-gray-500 mr-2">Last Sync:</span>
              <span className="font-medium text-gray-900">
                {device.lastSynchronization ? formatDistanceToNow(new Date(device.lastSynchronization), { addSuffix: true }) : 'Never'}
              </span>
            </div>
          </div>
          <div className="flex items-center gap-3">
            <MapPin className="h-4 w-4 text-gray-400" />
            <div className="text-sm">
              <span className="text-gray-500 mr-2">Location:</span>
              <span className="font-medium text-gray-900">
                Lat {device.latitude}, Lng {device.longitude}
              </span>
            </div>
          </div>
        </div>

        <div>
          <h4 className="text-sm font-medium text-gray-900 mb-3">Supported Features</h4>
          <div className="flex flex-wrap gap-2">
            {capabilities.map(cap => (
              <Badge 
                key={cap.name} 
                variant={cap.enabled ? "secondary" : "outline"}
                className={cap.enabled ? "bg-blue-50 text-blue-700" : "text-gray-400"}
              >
                {cap.name}
              </Badge>
            ))}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
