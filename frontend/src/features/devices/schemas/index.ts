import { z } from 'zod';
import { DeviceType, CommunicationProtocol } from '../types';

export const deviceSchema = z.object({
  name: z.string().min(1, 'Device name is required').max(100, 'Name must be at most 100 characters'),
  code: z.string().min(1, 'Device code is required').max(50, 'Code must be at most 50 characters'),
  serialNumber: z.string().max(100).optional().or(z.literal('')),
  deviceType: z.nativeEnum(DeviceType),
  operatingSystem: z.string().max(50).optional().or(z.literal('')),
  applicationVersion: z.string().max(50).optional().or(z.literal('')),
  branchId: z.string().optional().or(z.literal('')),
  
  // Location
  latitude: z.coerce.number().min(-90).max(90).default(0),
  longitude: z.coerce.number().min(-180).max(180).default(0),
  allowedRadiusMeters: z.coerce.number().min(0).default(0),

  // Network
  ipAddress: z.string()
    .refine(val => !val || /^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$/.test(val), 'Invalid IP Address')
    .optional().or(z.literal('')),
  port: z.coerce.number().min(1).max(65535).optional().or(z.literal('')),
  communicationProtocol: z.nativeEnum(CommunicationProtocol).default(CommunicationProtocol.HTTPS),

  // Security
  publicKey: z.string().optional().or(z.literal('')),
  deviceFingerprint: z.string().optional().or(z.literal('')),
  requireDeviceApproval: z.boolean().default(false),
  requireSecureConnection: z.boolean().default(true),

  // Capabilities
  supportsFaceRecognition: z.boolean().default(false),
  supportsFingerprint: z.boolean().default(false),
  supportsGPS: z.boolean().default(false),
  supportsOfflineAttendance: z.boolean().default(false),
  supportsQRCode: z.boolean().default(false),
  supportsNFC: z.boolean().default(false),
});

export type DeviceFormValues = z.infer<typeof deviceSchema>;
