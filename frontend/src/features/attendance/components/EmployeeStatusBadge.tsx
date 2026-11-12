import { EmployeeAttendanceState } from '../types';

interface EmployeeStatusBadgeProps {
  state: string | undefined;
  size?: 'sm' | 'md' | 'lg';
}

interface StateConfig {
  label: string;
  emoji: string;
  bgClass: string;
  textClass: string;
  ringClass: string;
  dotClass: string;
}

function getStateConfig(state: string | undefined): StateConfig {
  switch (state) {
    case EmployeeAttendanceState.Working:
      return {
        label: 'Working',
        emoji: '🟢',
        bgClass: 'bg-emerald-50 dark:bg-emerald-900/20',
        textClass: 'text-emerald-700 dark:text-emerald-400',
        ringClass: 'ring-emerald-200 dark:ring-emerald-700',
        dotClass: 'bg-emerald-500',
      };
    case EmployeeAttendanceState.OnBreak:
      return {
        label: 'On Break',
        emoji: '🟡',
        bgClass: 'bg-amber-50 dark:bg-amber-900/20',
        textClass: 'text-amber-700 dark:text-amber-400',
        ringClass: 'ring-amber-200 dark:ring-amber-700',
        dotClass: 'bg-amber-400',
      };
    case EmployeeAttendanceState.Finished:
      return {
        label: 'Finished',
        emoji: '🔴',
        bgClass: 'bg-rose-50 dark:bg-rose-900/20',
        textClass: 'text-rose-700 dark:text-rose-400',
        ringClass: 'ring-rose-200 dark:ring-rose-700',
        dotClass: 'bg-rose-500',
      };
    case EmployeeAttendanceState.NotCheckedIn:
    default:
      return {
        label: 'Not Checked In',
        emoji: '⚪',
        bgClass: 'bg-gray-50 dark:bg-gray-800',
        textClass: 'text-gray-500 dark:text-gray-400',
        ringClass: 'ring-gray-200 dark:ring-gray-600',
        dotClass: 'bg-gray-400',
      };
  }
}

/**
 * Displays the current live attendance state of an employee as a coloured badge.
 *
 * | State         | Colour | Icon |
 * |---------------|--------|------|
 * | Working       | Green  | 🟢   |
 * | On Break      | Amber  | 🟡   |
 * | Finished      | Red    | 🔴   |
 * | Not Checked In| Gray   | ⚪   |
 */
export function EmployeeStatusBadge({ state, size = 'md' }: EmployeeStatusBadgeProps) {
  const config = getStateConfig(state);

  const sizeClass = {
    sm: 'text-xs px-2 py-0.5 gap-1',
    md: 'text-sm px-3 py-1 gap-1.5',
    lg: 'text-base px-4 py-1.5 gap-2',
  }[size];

  const dotSize = {
    sm: 'w-1.5 h-1.5',
    md: 'w-2 h-2',
    lg: 'w-2.5 h-2.5',
  }[size];

  return (
    <span
      className={`inline-flex items-center rounded-full font-medium ring-1 ring-inset transition-colors
        ${sizeClass} ${config.bgClass} ${config.textClass} ${config.ringClass}`}
      role="status"
      aria-label={`Attendance status: ${config.label}`}
    >
      {/* Animated dot for "Working" state */}
      {state === EmployeeAttendanceState.Working ? (
        <span className="relative flex items-center justify-center">
          <span
            className={`animate-ping absolute inline-flex rounded-full opacity-60 ${dotSize} ${config.dotClass}`}
          />
          <span className={`relative inline-flex rounded-full ${dotSize} ${config.dotClass}`} />
        </span>
      ) : (
        <span className={`inline-flex rounded-full flex-shrink-0 ${dotSize} ${config.dotClass}`} />
      )}
      {config.label}
    </span>
  );
}
