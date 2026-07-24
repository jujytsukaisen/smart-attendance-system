import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Checkbox } from '@/shared/components/ui/checkbox';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/shared/components/ui/form';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { deviceSchema, type DeviceFormValues } from '../schemas';
import { DeviceType, CommunicationProtocol } from '../types';

interface DeviceFormProps {
  initialValues?: Partial<DeviceFormValues>;
  onSubmit: (values: DeviceFormValues) => void;
  isLoading?: boolean;
}

const defaultValues: DeviceFormValues = {
  name: '',
  code: '',
  serialNumber: '',
  deviceType: DeviceType.WallMounted,
  operatingSystem: '',
  applicationVersion: '',
  branchId: '',
  latitude: 0,
  longitude: 0,
  allowedRadiusMeters: 0,
  ipAddress: '',
  port: 80,
  communicationProtocol: CommunicationProtocol.HTTPS,
  publicKey: '',
  deviceFingerprint: '',
  requireDeviceApproval: false,
  requireSecureConnection: true,
  supportsFaceRecognition: false,
  supportsFingerprint: false,
  supportsGPS: false,
  supportsOfflineAttendance: false,
  supportsQRCode: false,
  supportsNFC: false,
};

export function DeviceForm({ initialValues, onSubmit, isLoading }: DeviceFormProps) {
  const form = useForm<DeviceFormValues>({
    resolver: zodResolver(deviceSchema) as any,
    defaultValues: { ...defaultValues, ...initialValues },
  });

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-8">
        {/* General Information */}
        <div className="bg-white p-6 rounded-lg shadow border border-gray-200">
          <h3 className="text-lg font-medium text-gray-900 mb-4">General Information</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <FormField control={form.control} name="name" render={({ field }) => (
              <FormItem><FormLabel>Device Name</FormLabel><FormControl><Input placeholder="Front Door Scanner" {...field} /></FormControl><FormMessage /></FormItem>
            )} />
            <FormField control={form.control} name="code" render={({ field }) => (
              <FormItem><FormLabel>Device Code</FormLabel><FormControl><Input placeholder="DEV-001" {...field} /></FormControl><FormMessage /></FormItem>
            )} />
            <FormField control={form.control} name="serialNumber" render={({ field }) => (
              <FormItem><FormLabel>Serial Number</FormLabel><FormControl><Input placeholder="SN-123456" {...field} /></FormControl><FormMessage /></FormItem>
            )} />
            <FormField control={form.control} name="deviceType" render={({ field }) => (
              <FormItem>
                <FormLabel>Device Type</FormLabel>
                <Select onValueChange={(val) => field.onChange(Number(val))} value={field.value?.toString()}>
                  <FormControl><SelectTrigger><SelectValue placeholder="Select type" /></SelectTrigger></FormControl>
                  <SelectContent>
                    <SelectItem value={DeviceType.WallMounted.toString()}>Wall Mounted</SelectItem>
                    <SelectItem value={DeviceType.Mobile.toString()}>Mobile</SelectItem>
                    <SelectItem value={DeviceType.Desktop.toString()}>Desktop</SelectItem>
                    <SelectItem value={DeviceType.Kiosk.toString()}>Kiosk</SelectItem>
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )} />
          </div>
        </div>

        {/* Network Configuration */}
        <div className="bg-white p-6 rounded-lg shadow border border-gray-200">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Network Configuration</h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <FormField control={form.control} name="ipAddress" render={({ field }) => (
              <FormItem><FormLabel>IP Address</FormLabel><FormControl><Input placeholder="192.168.1.100" {...field} /></FormControl><FormMessage /></FormItem>
            )} />
            <FormField control={form.control} name="port" render={({ field }) => (
              <FormItem><FormLabel>Port</FormLabel><FormControl><Input type="number" {...field} /></FormControl><FormMessage /></FormItem>
            )} />
            <FormField control={form.control} name="communicationProtocol" render={({ field }) => (
              <FormItem>
                <FormLabel>Protocol</FormLabel>
                <Select onValueChange={(val) => field.onChange(Number(val))} value={field.value?.toString()}>
                  <FormControl><SelectTrigger><SelectValue placeholder="Select protocol" /></SelectTrigger></FormControl>
                  <SelectContent>
                    <SelectItem value={CommunicationProtocol.HTTP.toString()}>HTTP</SelectItem>
                    <SelectItem value={CommunicationProtocol.HTTPS.toString()}>HTTPS</SelectItem>
                    <SelectItem value={CommunicationProtocol.TCP.toString()}>TCP</SelectItem>
                    <SelectItem value={CommunicationProtocol.UDP.toString()}>UDP</SelectItem>
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )} />
          </div>
        </div>

        {/* Device Capabilities */}
        <div className="bg-white p-6 rounded-lg shadow border border-gray-200">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Device Capabilities</h3>
          <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
            <FormField control={form.control} name="supportsFaceRecognition" render={({ field }) => (
              <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
                <FormControl><Checkbox checked={field.value} onCheckedChange={field.onChange} /></FormControl>
                <div className="space-y-1 leading-none"><FormLabel>Face Recognition</FormLabel></div>
              </FormItem>
            )} />
            <FormField control={form.control} name="supportsFingerprint" render={({ field }) => (
              <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
                <FormControl><Checkbox checked={field.value} onCheckedChange={field.onChange} /></FormControl>
                <div className="space-y-1 leading-none"><FormLabel>Fingerprint</FormLabel></div>
              </FormItem>
            )} />
            <FormField control={form.control} name="supportsQRCode" render={({ field }) => (
              <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
                <FormControl><Checkbox checked={field.value} onCheckedChange={field.onChange} /></FormControl>
                <div className="space-y-1 leading-none"><FormLabel>QR Code Scanner</FormLabel></div>
              </FormItem>
            )} />
            <FormField control={form.control} name="supportsNFC" render={({ field }) => (
              <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
                <FormControl><Checkbox checked={field.value} onCheckedChange={field.onChange} /></FormControl>
                <div className="space-y-1 leading-none"><FormLabel>NFC / RFID</FormLabel></div>
              </FormItem>
            )} />
            <FormField control={form.control} name="supportsGPS" render={({ field }) => (
              <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
                <FormControl><Checkbox checked={field.value} onCheckedChange={field.onChange} /></FormControl>
                <div className="space-y-1 leading-none"><FormLabel>GPS location</FormLabel></div>
              </FormItem>
            )} />
            <FormField control={form.control} name="supportsOfflineAttendance" render={({ field }) => (
              <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
                <FormControl><Checkbox checked={field.value} onCheckedChange={field.onChange} /></FormControl>
                <div className="space-y-1 leading-none"><FormLabel>Offline Syncing</FormLabel></div>
              </FormItem>
            )} />
          </div>
        </div>

        <div className="flex justify-end pt-4">
          <Button type="submit" disabled={isLoading}>
            {isLoading ? 'Saving...' : 'Save Device'}
          </Button>
        </div>
      </form>
    </Form>
  );
}
