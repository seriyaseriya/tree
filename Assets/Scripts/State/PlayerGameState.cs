using System;

namespace WoodClicker.State
{
    [Serializable]
    public sealed class PlayerGameState
    {
        public double OwnedLogs { get; private set; }
        public double TotalLogsEarned { get; private set; }
        public long TotalManualChops { get; private set; }

        public void ApplyManualChop(double earnedLogs)
        {
            if (earnedLogs < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(earnedLogs));
            }

            OwnedLogs += earnedLogs;
            TotalLogsEarned += earnedLogs;
            TotalManualChops++;
        }
    }
}
