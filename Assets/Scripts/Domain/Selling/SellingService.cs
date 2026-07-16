using System;

namespace WoodClicker.Domain.Selling
{
    public sealed class SellingService
    {
        private readonly double _moneyPerLog;

        public SellingService(double moneyPerLog)
        {
            if (moneyPerLog < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(moneyPerLog));
            }

            _moneyPerLog = moneyPerLog;
        }

        public SellingResult SellAll(double ownedLogs)
        {
            if (ownedLogs < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(ownedLogs));
            }

            if (ownedLogs == 0d)
            {
                return new SellingResult(0d, 0d, SellingFailureReason.NoLogs);
            }

            return new SellingResult(
                ownedLogs,
                ownedLogs * _moneyPerLog,
                SellingFailureReason.None);
        }
    }
}
