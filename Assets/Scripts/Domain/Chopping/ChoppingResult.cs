namespace WoodClicker.Domain.Chopping
{
    public readonly struct ChoppingResult
    {
        public double EarnedLogs { get; }
        public float CooldownSeconds { get; }
        public ChoppingFailureReason FailureReason { get; }

        public bool Succeeded => FailureReason == ChoppingFailureReason.None;

        public ChoppingResult(
            double earnedLogs,
            float cooldownSeconds,
            ChoppingFailureReason failureReason)
        {
            EarnedLogs = earnedLogs;
            CooldownSeconds = cooldownSeconds;
            FailureReason = failureReason;
        }
    }
}
