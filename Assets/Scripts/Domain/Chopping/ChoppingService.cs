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

        public ChoppingResult TryChop(
            bool isCooldownActive,
            bool isTreeFelled)
        {
            if (isTreeFelled)
            {
                return new ChoppingResult(
                    0d,
                    0d,
                    0f,
                    ChoppingFailureReason.TreeAlreadyFelled);
            }

            if (isCooldownActive)
            {
                return new ChoppingResult(
                    0d,
                    0d,
                    0f,
                    ChoppingFailureReason.CooldownActive);
            }

            return new ChoppingResult(
                _config.LogsPerChop,
                _config.LogsPerChop,
                _config.CooldownSeconds,
                ChoppingFailureReason.None);
        }
    }
}
