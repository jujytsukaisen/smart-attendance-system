import { format } from 'date-fns';
import {
  LogIn,
  Coffee,
  ArrowLeftRight,
  LogOut,
} from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import type { AttendanceEventDto } from '../types';

interface AttendanceTimelineProps {
  events: AttendanceEventDto[];
}

interface EventConfig {
  label: string;
  icon: React.ReactNode;
  dotClass: string;
  labelClass: string;
  lineClass: string;
}

function getEventConfig(eventType: string): EventConfig {
  const t = eventType?.toLowerCase();
  switch (t) {
    case 'checkin':
      return {
        label: 'Check In',
        icon: <LogIn className="h-4 w-4" />,
        dotClass: 'bg-emerald-500 border-emerald-200',
        labelClass: 'text-emerald-700 dark:text-emerald-400',
        lineClass: 'bg-emerald-100 dark:bg-emerald-900/30',
      };
    case 'breakstart':
      return {
        label: 'Break Start',
        icon: <Coffee className="h-4 w-4" />,
        dotClass: 'bg-amber-400 border-amber-100',
        labelClass: 'text-amber-700 dark:text-amber-400',
        lineClass: 'bg-amber-50 dark:bg-amber-900/20',
      };
    case 'breakend':
      return {
        label: 'Break End',
        icon: <ArrowLeftRight className="h-4 w-4" />,
        dotClass: 'bg-sky-500 border-sky-100',
        labelClass: 'text-sky-700 dark:text-sky-400',
        lineClass: 'bg-sky-50 dark:bg-sky-900/20',
      };
    case 'checkout':
      return {
        label: 'Check Out',
        icon: <LogOut className="h-4 w-4" />,
        dotClass: 'bg-rose-500 border-rose-200',
        labelClass: 'text-rose-700 dark:text-rose-400',
        lineClass: 'bg-rose-50 dark:bg-rose-900/20',
      };
    default:
      return {
        label: eventType,
        icon: <ArrowLeftRight className="h-4 w-4" />,
        dotClass: 'bg-gray-400 border-gray-200',
        labelClass: 'text-gray-600',
        lineClass: 'bg-gray-50',
      };
  }
}

function formatEventTime(ts: string): string {
  let s = ts.trim();
  if (s.includes('T') && !s.endsWith('Z') && !s.includes('+') && !s.match(/-\d\d:\d\d$/)) {
    s += 'Z';
  }
  const d = new Date(s);
  if (isNaN(d.getTime())) return ts;
  return format(d, 'h:mm a');
}

/**
 * Renders the daily attendance event timeline.
 * Displays each scan in chronological order with colour-coded icons:
 *   🟢 Check In  →  🟡 Break Start  →  🔵 Break End  →  🔴 Check Out
 */
export function AttendanceTimeline({ events }: AttendanceTimelineProps) {
  if (!events || events.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Daily Timeline</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground text-center py-6">
            No events recorded yet for this attendance record.
          </p>
        </CardContent>
      </Card>
    );
  }

  // Sort ascending by timestamp (should already be sorted, but defensive)
  const sorted = [...events].sort(
    (a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime()
  );

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg">Daily Timeline</CardTitle>
      </CardHeader>
      <CardContent>
        <ol className="relative">
          {sorted.map((event, index) => {
            const config = getEventConfig(event.eventType as string);
            const isLast = index === sorted.length - 1;

            return (
              <li key={event.id} className="flex gap-4 group">
                {/* Timeline spine */}
                <div className="flex flex-col items-center">
                  {/* Dot */}
                  <span
                    className={`flex-shrink-0 w-9 h-9 rounded-full border-4 flex items-center justify-center text-white shadow-sm transition-transform group-hover:scale-110 ${config.dotClass}`}
                    aria-label={config.label}
                  >
                    {config.icon}
                  </span>
                  {/* Vertical connector */}
                  {!isLast && (
                    <span className="w-0.5 flex-1 bg-border my-1" aria-hidden="true" />
                  )}
                </div>

                {/* Event card */}
                <div
                  className={`mb-4 flex-1 rounded-lg px-4 py-3 border border-border/60 transition-colors ${config.lineClass}`}
                >
                  <div className="flex items-center justify-between">
                    <span className={`text-sm font-semibold ${config.labelClass}`}>
                      {config.label}
                    </span>
                    <span className="text-xs font-mono text-muted-foreground tabular-nums">
                      {formatEventTime(event.timestamp)}
                    </span>
                  </div>
                  {event.note && (
                    <p className="mt-1 text-xs text-muted-foreground italic">{event.note}</p>
                  )}
                </div>
              </li>
            );
          })}
        </ol>
      </CardContent>
    </Card>
  );
}
