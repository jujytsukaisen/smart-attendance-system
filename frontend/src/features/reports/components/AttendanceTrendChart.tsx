import {
  AreaChart,
  Area,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Legend
} from 'recharts';
import type { AttendanceTrendData } from '../types';
import { useTranslation } from '@/shared/i18n';

interface AttendanceTrendChartProps {
  data: AttendanceTrendData[];
  height?: number;
}

export function AttendanceTrendChart({ data, height = 350 }: AttendanceTrendChartProps) {
  const { t } = useTranslation();

  return (
    <div style={{ height, width: '100%' }}>
      <ResponsiveContainer>
        <AreaChart
          data={data}
          margin={{ top: 10, right: 30, left: 0, bottom: 0 }}
        >
          <defs>
            <linearGradient id="colorPresent" x1="0" y1="0" x2="0" y2="1">
              <stop offset="5%" stopColor="#10b981" stopOpacity={0.8}/>
              <stop offset="95%" stopColor="#10b981" stopOpacity={0}/>
            </linearGradient>
            <linearGradient id="colorLate" x1="0" y1="0" x2="0" y2="1">
              <stop offset="5%" stopColor="#f59e0b" stopOpacity={0.8}/>
              <stop offset="95%" stopColor="#f59e0b" stopOpacity={0}/>
            </linearGradient>
          </defs>
          <CartesianGrid strokeDasharray="3 3" vertical={false} className="stroke-muted" />
          <XAxis dataKey="date" tickLine={false} axisLine={false} tickMargin={10} className="text-xs" />
          <YAxis tickLine={false} axisLine={false} tickMargin={10} className="text-xs" />
          <Tooltip 
            contentStyle={{ 
              backgroundColor: 'hsl(var(--card))', 
              borderColor: 'hsl(var(--border))', 
              borderRadius: '8px', 
              color: 'hsl(var(--card-foreground))' 
            }}
          />
          <Legend />
          <Area 
            type="monotone" 
            dataKey="present" 
            name={t('attendance.present')} 
            stroke="#10b981" 
            fillOpacity={1} 
            fill="url(#colorPresent)" 
            strokeWidth={2}
          />
          <Area 
            type="monotone" 
            dataKey="late" 
            name={t('attendance.late')} 
            stroke="#f59e0b" 
            fillOpacity={1} 
            fill="url(#colorLate)" 
            strokeWidth={2}
          />
          <Area 
            type="monotone" 
            dataKey="absent" 
            name={t('attendance.absent')} 
            stroke="#ef4444" 
            fillOpacity={0.1} 
            fill="#ef4444" 
          />
        </AreaChart>
      </ResponsiveContainer>
    </div>
  );
}
