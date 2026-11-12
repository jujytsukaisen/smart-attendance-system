export interface ShiftCandidate {
  id: string;
  name: string;
  code?: string;
  startTime: string; // "HH:mm" or "HH:mm:ss"
  endTime: string;   // "HH:mm" or "HH:mm:ss"
  shiftType?: number;
  isNightShift?: boolean;
  allowEarlyCheckInMinutes?: number;
  allowLateCheckOutMinutes?: number;
  gracePeriodMinutes?: number;
  isActive?: boolean;
}

/**
 * Parses time string "08:00" or "08:00:00" into total minutes from midnight (0 - 1439).
 */
export function timeStringToMinutes(timeStr: string): number {
  if (!timeStr) return 0;
  const parts = timeStr.split(':');
  const hours = parseInt(parts[0] || '0', 10);
  const minutes = parseInt(parts[1] || '0', 10);
  return ((hours * 60 + minutes) % 1440 + 1440) % 1440;
}

/**
 * Automatically detects the best matching active shift for a given Date / Time.
 * Considers shift start/end times, early check-in allowances, night shifts crossing midnight,
 * and proximity to the start time.
 */
export function detectBestShift<T extends ShiftCandidate>(
  shifts: T[],
  currentDate: Date = new Date()
): T | undefined {
  if (!shifts || shifts.length === 0) return undefined;

  const activeShifts = shifts.filter((s) => s.isActive !== false);
  if (activeShifts.length === 0) return undefined;
  if (activeShifts.length === 1) return activeShifts[0];

  const nowMinutes = currentDate.getHours() * 60 + currentDate.getMinutes();

  const scored = activeShifts.map((shift) => {
    const startMin = timeStringToMinutes(shift.startTime);
    const endMin = timeStringToMinutes(shift.endTime);
    const isNight = shift.isNightShift || startMin > endMin;

    const earlyBuffer = Math.max(0, shift.allowEarlyCheckInMinutes ?? 30);
    const lateBuffer = Math.max(0, shift.allowLateCheckOutMinutes ?? 30);

    let isInWindow = false;

    if (!isNight) {
      const windowStart = (startMin - earlyBuffer + 1440) % 1440;
      const windowEnd = endMin + lateBuffer;

      if (windowEnd < 1440 && windowStart <= windowEnd) {
        isInWindow = nowMinutes >= windowStart && nowMinutes <= windowEnd;
      } else {
        const normalizedEnd = windowEnd % 1440;
        isInWindow = nowMinutes >= windowStart || nowMinutes <= normalizedEnd;
      }
    } else {
      // Night shift crosses midnight
      const windowStart = (startMin - earlyBuffer + 1440) % 1440;
      const windowEnd = (endMin + lateBuffer) % 1440;
      isInWindow = nowMinutes >= windowStart || nowMinutes <= windowEnd;
    }

    // Circular distance to start time in 24 hours (0 - 720 min)
    const diff = Math.abs(nowMinutes - startMin);
    const circularDiff = Math.min(diff, 1440 - diff);

    // Shifts currently in window get heavy priority (-10000)
    let score = circularDiff;
    if (isInWindow) {
      score -= 10000;
    }

    return {
      shift,
      score,
      isInWindow,
    };
  });

  scored.sort((a, b) => a.score - b.score);
  return scored[0]?.shift;
}
