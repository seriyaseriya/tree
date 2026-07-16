using System;

namespace WoodClicker.Domain.Chopping
{
    public sealed class ChoppingConfig
    {
        public double LogsPerChop { get; }
        public float CooldownSeconds { get; }

        public ChoppingConfig(double logsPerChop, float cooldownSeconds)
        {
            if (logsPerChop < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(logsPerChop));
            }

            if (cooldownSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(cooldownSeconds));
            }

            LogsPerChop = logsPerChop;
            CooldownSeconds = cooldownSeconds;
        }
    }
}
