namespace WoodClicker.Domain.Selling
{
    public readonly struct SellingResult
    {
        public double SoldLogs { get; }
        public double EarnedMoney { get; }
        public SellingFailureReason FailureReason { get; }

        public bool Succeeded => FailureReason == SellingFailureReason.None;

        public SellingResult(
            double soldLogs,
            double earnedMoney,
            SellingFailureReason failureReason)
        {
            SoldLogs = soldLogs;
            EarnedMoney = earnedMoney;
            FailureReason = failureReason;
        }
    }
}
