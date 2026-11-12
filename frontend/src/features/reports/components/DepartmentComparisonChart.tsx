import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Legend
} from 'recharts';
import { useTranslation } from '@/shared/i18n';

interface DepartmentComparisonChartProps {
  data: { name: string; present: number; absent: number; late: number }[];
  height?: number;
}

export function DepartmentComparisonChart({ data, height = 350 }: DepartmentComparisonChartProps) {
  const { t } = useTranslation();

  return (
    <div style={{ height, width: '100%' }}>
      <ResponsiveContainer>
        <BarChart
          data={data}
          margin={{ top: 20, right: 30, left: 0, bottom: 5 }}
        >
          <CartesianGrid strokeDasharray="3 3" vertical={false} className="stroke-muted" />
          <XAxis dataKey="name" tickLine={false} axisLine={false} className="text-xs" />
          <YAxis tickLine={false} axisLine={false} className="text-xs" />
          <Tooltip 
            cursor={{ fill: 'transparent' }}
            contentStyle={{ 
              backgroundColor: 'hsl(var(--card))', 
              borderColor: 'hsl(var(--border))', 
              borderRadius: '8px', 
              color: 'hsl(var(--card-foreground))' 
            }}
          />
          <Legend />
          <Bar dataKey="present" name={t('attendance.present')} stackId="a" fill="#10b981" radius={[0, 0, 4, 4]} />
          <Bar dataKey="late" name={t('attendance.late')} stackId="a" fill="#f59e0b" />
          <Bar dataKey="absent" name={t('attendance.absent')} stackId="a" fill="#ef4444" radius={[4, 4, 0, 0]} />
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
