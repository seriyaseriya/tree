namespace WoodClicker.Domain.Chopping
{
    public readonly struct ChoppingResult
    {
        public double EarnedLogs { get; }
        public double TreeDamage { get; }
        public float CooldownSeconds { get; }
        public ChoppingFailureReason FailureReason { get; }

        public bool Succeeded => FailureReason == ChoppingFailureReason.None;

        public ChoppingResult(
            double earnedLogs,
            double treeDamage,
            float cooldownSeconds,
            ChoppingFailureReason failureReason)
        {
            EarnedLogs = earnedLogs;
            TreeDamage = treeDamage;
            CooldownSeconds = cooldownSeconds;
            FailureReason = failureReason;
        }
    }
}
