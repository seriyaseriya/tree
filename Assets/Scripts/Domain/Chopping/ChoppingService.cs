using System;

namespace WoodClicker.Domain.Chopping
{
    public sealed class ChoppingService
    {
        private readonly ChoppingConfig _config;

        public ChoppingService(ChoppingConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public ChoppingResult TryChop(bool isCooldownActive)
        {
            if (isCooldownActive)
            {
                return new ChoppingResult(
                    0d,
                    0f,
                    ChoppingFailureReason.CooldownActive);
            }

            return new ChoppingResult(
                _config.LogsPerChop,
                _config.CooldownSeconds,
                ChoppingFailureReason.None);
        }
    }
}
