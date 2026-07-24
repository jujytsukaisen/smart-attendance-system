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

interface DepartmentComparisonChartProps {
  data: { name: string; present: number; absent: number; late: number }[];
  height?: number;
}

export function DepartmentComparisonChart({ data, height = 350 }: DepartmentComparisonChartProps) {
  return (
    <div style={{ height, width: '100%' }}>
      <ResponsiveContainer>
        <BarChart
          data={data}
          margin={{ top: 20, right: 30, left: 0, bottom: 5 }}
        >
          <CartesianGrid strokeDasharray="3 3" vertical={false} />
          <XAxis dataKey="name" tickLine={false} axisLine={false} />
          <YAxis tickLine={false} axisLine={false} />
          <Tooltip 
            cursor={{ fill: 'transparent' }}
            contentStyle={{ borderRadius: '8px', border: 'none', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
          />
          <Legend />
          <Bar dataKey="present" name="Present" stackId="a" fill="#10b981" radius={[0, 0, 4, 4]} />
          <Bar dataKey="late" name="Late" stackId="a" fill="#f59e0b" />
          <Bar dataKey="absent" name="Absent" stackId="a" fill="#ef4444" radius={[4, 4, 0, 0]} />
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
